import socket
import threading
import numpy as np
from sklearn.linear_model import SGDClassifier
import joblib
import os
from torch.utils.tensorboard import SummaryWriter

# === Configurazione ===
HOST, PORT = "localhost", 5005
MAX_CLIENTS = 6
SAVE_EVERY = 10000
LOG_EVERY = 100
MODEL_PATH = "models/trained_model.pkl"
TENSORBOARD_LOGDIR = "runs/sgd_classifier_logs"

# === Stato globale ===
first_fit = True
step_count = 0
correct_predictions = 0
losses = []
lock = threading.Lock()
writer = SummaryWriter(TENSORBOARD_LOGDIR)

os.makedirs("models", exist_ok=True)

# === Caricamento modello se esistente ===
if os.path.exists(MODEL_PATH):
    print(f"📂 Trovato modello esistente. Caricamento da {MODEL_PATH}...")
    model = joblib.load(MODEL_PATH)
    first_fit = True
    print("✅ Modello caricato.")
else:
    print("📄 Nessun modello trovato. Creo un nuovo SGDClassifier.")
    model = SGDClassifier(loss="log_loss")
    first_fit = True

classes = np.array([0, 1])  # 0 = notEnemy, 1 = Enemy

def save_model():
    joblib.dump(model, MODEL_PATH)
    print(f"💾 Modello salvato dopo {step_count} passi in {MODEL_PATH}")

def handle_client(conn, addr):
    global first_fit, step_count, correct_predictions, losses

    print(f"👤 Client connesso da {addr}")

    try:
        while True:
            data = conn.recv(1024).decode()
            if not data:
                print(f"🔌 Client {addr} ha chiuso la connessione.")
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

                    y_scalar = int(y.item())  # 0 o 1
                    probas = model.predict_proba(X)[0]

                    if len(probas) < 2:
                        proba = 0.5
                    else:
                        class_index = y_scalar
                        proba = float(probas[class_index])

                    loss = float(-np.log(np.clip(proba, 1e-10, 1)))
                    losses.append(loss)

                    if pred == y_scalar:
                        correct_predictions += 1

                    print(f"[{step_count}] Pred: {pred} | TrueLabel: {y_scalar} | {'✓' if pred == y_scalar else '✗'}")

                    if step_count % LOG_EVERY == 0:
                        accuracy = (correct_predictions / step_count) * 100
                        avg_loss = np.mean(losses[-LOG_EVERY:]) * 100
                        writer.add_scalar("Accuracy (%)", accuracy, step_count)
                        writer.add_scalar("Loss (%)", avg_loss, step_count)
                        print(f"📊 STEP {step_count} | Accuracy: {accuracy:.2f}% | Loss: {avg_loss:.2f}%")

                    if step_count % SAVE_EVERY == 0:
                        save_model()
                else:
                    pred = int(model.predict(X)[0])

            conn.send((str(pred) + "\n").encode())

    except Exception as e:
        print(f"❌ Errore client {addr}: {e}")

    finally:
        conn.close()
        print(f"❎ Connessione chiusa con {addr}")

# === Avvio server principale ===
server = socket.socket()
server.bind((HOST, PORT))
server.listen(MAX_CLIENTS)

print(f"🧠 Server in ascolto su {HOST}:{PORT} (max {MAX_CLIENTS} client)")

threads = []

try:
    while True:
        conn, addr = server.accept()
        t = threading.Thread(target=handle_client, args=(conn, addr))
        t.start()
        threads.append(t)

        if len(threads) >= MAX_CLIENTS:
            print(f"🚫 Raggiunto il massimo di {MAX_CLIENTS} client.")
            break

except KeyboardInterrupt:
    print("🛑 Interruzione manuale.")

finally:
    server.close()
    for t in threads:
        t.join()
    save_model()
    writer.close()
    print("✅ Server terminato.")
