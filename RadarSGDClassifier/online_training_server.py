
import socket
import threading
import numpy as np
from sklearn.linear_model import SGDClassifier
import joblib
import os
import time
from collections import deque
from tensorboardX import SummaryWriter
from rich.live import Live
from rich.table import Table
from rich.columns import Columns
from rich.panel import Panel
from rich.progress import Progress, BarColumn, TextColumn
from rich.layout import Layout

# === Config ===
HOST, PORT = "localhost", 5005
MAX_CLIENTS = 6
SAVE_EVERY = 10000
LOG_EVERY = 100
MODEL_PATH = "models/trained_model.pkl"
STATE_PATH = "models/checkpoint.npz"
TENSORBOARD_LOGDIR = "runs/sgd_classifier_logs"

first_fit = True
step_count = 0
correct_predictions = 0
losses = []
recent_preds = deque(maxlen=100)
conf_matrix = np.zeros((2, 2), dtype=int)
start_time = time.time()
lock = threading.Lock()
writer = SummaryWriter(TENSORBOARD_LOGDIR)

os.makedirs("models", exist_ok=True)

# === Load model if existing ===
if os.path.exists(MODEL_PATH):
    print(f" Existing model found. Loading from {MODEL_PATH}...")
    model = joblib.load(MODEL_PATH)
    first_fit = True
    print("Model loaded.")
else:
    print("No model found. Creating a new SGDClassifier.")
    model = SGDClassifier(loss="log_loss")
    first_fit = True

# === Loading training state if existing ===
if os.path.exists(STATE_PATH):
    state = np.load(STATE_PATH, allow_pickle=True)
    step_count = int(state["step_count"])
    correct_predictions = int(state["correct_predictions"])
    losses = state["losses"].tolist()
    print(f"State resumed: step={step_count}, accuracy={(correct_predictions / step_count) * 100:.2f}%")
else:
    print("No checkpoint found, starting new training.")

classes = np.array([0, 1])  # 0 = notEnemy, 1 = Enemy

def save_model():
    joblib.dump(model, MODEL_PATH)
    np.savez(STATE_PATH,
             step_count=step_count,
             correct_predictions=correct_predictions,
             losses=np.array(losses, dtype=float))
    print(f"Model and state saved after {step_count} steps")

def create_dashboard(step, correct, accuracy, loss, velocity, elapsed, conf_matrix):
    wrong = step - correct
    recent_correct = sum(recent_preds)
    recent_wrong = len(recent_preds) - recent_correct

    # Progress bar with accuracy
    progress_bar = Progress(
        TextColumn("Accuracy"),
        BarColumn(bar_width=20),
        TextColumn(f"{accuracy:.2f}%")
    )
    progress_bar.add_task("", total=100, completed=accuracy)

    # Main info
    stats = Table.grid()
    stats.add_row(f" Step: {step} | ✔️ {correct} | ❌ {wrong}")
    stats.add_row(f" {int(elapsed // 60)}m {int(elapsed % 60)}s | {velocity:.2f} step/s")
    stats.add_row(f" Loss: {loss:.2f}%")
    stats.add_row(f"Last 100: ✔️ {recent_correct} | ❌ {recent_wrong}")

    # Confusion Matrix
    cm = Table(title="Confusion Matrix", box=None)
    cm.add_column(" ", justify="right")
    cm.add_column("Pred 0", justify="center")
    cm.add_column("Pred 1", justify="center")
    cm.add_row("True 0", str(conf_matrix[0][0]), str(conf_matrix[0][1]))
    cm.add_row("True 1", str(conf_matrix[1][0]), str(conf_matrix[1][1]))

    columns = Columns([
        Panel(progress_bar.get_renderable(), title="Performance"),
        Panel(stats, title="Training Stats"),
        Panel(cm, title="Confusion Matrix")
    ])
    return columns

def handle_client(conn, addr, live):
    global first_fit, step_count, correct_predictions, losses

    print(f"Client connected from addr: {addr}")
    last_time = time.time()

    try:
        while True:
            data = conn.recv(1024).decode()
            if not data:
                print(f"Connection dropped from client {addr}.")
                break

            parts = data.strip().split("|")
            features = list(map(float, parts[:11]))
            X = np.array([features])
            y_str = parts[11].strip()
            y = np.array([int(y_str)], dtype=int)

            with lock:
                if y is not None:
                    if first_fit:
                        model.partial_fit(X, y, classes=classes)
                        first_fit = False
                    else:
                        model.partial_fit(X, y)

                    step_count += 1
                    pred = int(model.predict(X)[0])
                    y_scalar = int(y.item())
                    probas = model.predict_proba(X)[0]

                    if len(probas) < 2:
                        proba = 0.5
                    else:
                        proba = float(probas[y_scalar])

                    loss = float(-np.log(np.clip(proba, 1e-10, 1)))
                    losses.append(loss)

                    is_correct = pred == y_scalar
                    recent_preds.append(is_correct)
                    if is_correct:
                        correct_predictions += 1

                    # Update confusion matrix
                    conf_matrix[y_scalar][pred] += 1

                    accuracy = (correct_predictions / step_count) * 100
                    avg_loss = np.mean(losses[-LOG_EVERY:]) * 100 if step_count >= LOG_EVERY else np.mean(losses) * 100
                    velocity = 1 / (time.time() - last_time + 1e-6)
                    elapsed = time.time() - start_time
                    last_time = time.time()

                    writer.add_scalar("Accuracy (%)", accuracy, step_count)
                    writer.add_scalar("Loss (%)", avg_loss, step_count)

                    live.update(create_dashboard(step_count, correct_predictions, accuracy, avg_loss, velocity, elapsed, conf_matrix))

                    if step_count % SAVE_EVERY == 0:
                        save_model()

                else:
                    pred = int(model.predict(X)[0])

            conn.send((str(pred) + "\n").encode())

    except Exception as e:
        print(f"❌ Client error {addr}: {e}")

    finally:
        conn.close()
        print(f"❎ Connection closed with client {addr}")

# === Starting server ===
server = socket.socket()
server.bind((HOST, PORT))
server.listen(MAX_CLIENTS)

print(f"Server listening on {HOST}:{PORT} (max {MAX_CLIENTS} client)")

threads = []

with Live(console=None, refresh_per_second=4) as live:
    try:
        while True:
            conn, addr = server.accept()
            t = threading.Thread(target=handle_client, args=(conn, addr, live))
            t.start()
            threads.append(t)

            if len(threads) >= MAX_CLIENTS:
                print(f"Reached {MAX_CLIENTS} client limit.")
                break

    except KeyboardInterrupt:
        print("🛑 Manual interrupt.")

    finally:
        server.close()
        for t in threads:
            t.join()
        save_model()
        writer.close()
        print("✅ Server terminated.")
