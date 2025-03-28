import socket
import threading
import time
import numpy as np
from sklearn.linear_model import SGDClassifier
import joblib
import os

# === Configurazione ===
HOST, PORT = "localhost", 5005
MAX_CLIENTS = 6
CONNECTION_TIMEOUT = 30  # sec per attendere il primo client
INACTIVITY_TIMEOUT = 30  # sec per ogni client
SAVE_EVERY = 10000
MODEL_PATH = "models/trained_model.pkl"

# === Stato globale ===
model = SGDClassifier(loss="log_loss")
classes = np.array([0, 1])
first_fit = True
step_count = 0
lock = threading.Lock()  # Per sincronizzare accesso a model

os.makedirs("models", exist_ok=True)

def save_model():
    joblib.dump(model, MODEL_PATH)
    print(f"💾 Modello salvato dopo {step_count} passi in {MODEL_PATH}")

def handle_client(conn, addr):
    global first_fit, step_count

    print(f"👤 Client connesso da {addr}")
    conn.settimeout(INACTIVITY_TIMEOUT)

    try:
        while True:
            data = conn.recv(1024).decode()
            
            parts = list(map(float, data.strip().split(",")))

            X = np.array([parts[:10]])
            y = np.array([int(parts[10])]) if len(parts) > 10 else None

            with lock:
                if y is not None:
                    if first_fit:
                        model.partial_fit(X, y, classes=classes)
                        first_fit = False
                    else:
                        model.partial_fit(X, y)
                    step_count += 1
                    if step_count % SAVE_EVERY == 0:
                        save_model()

                pred = model.predict(X)[0]

            conn.send((str(pred) + "\n").encode())

    except socket.timeout:
        print(f"⏳ Timeout inattività client {addr} ({INACTIVITY_TIMEOUT}s).")

    except Exception as e:
        print(f"❌ Errore client {addr}: {e}")

    finally:
        conn.close()
        print(f"❎ Connessione chiusa con {addr}")

# === Avvio server principale ===
server = socket.socket()
server.bind((HOST, PORT))
server.listen(MAX_CLIENTS)
server.settimeout(CONNECTION_TIMEOUT)

print(f"🧠 Server in ascolto su {HOST}:{PORT} (max {MAX_CLIENTS} client)")

threads = []

try:
    start_time = time.time()
    while True:
        try:
            conn, addr = server.accept()
            t = threading.Thread(target=handle_client, args=(conn, addr))
            t.start()
            threads.append(t)

            # Interrompi accettazione se abbiamo raggiunto il massimo
            if len(threads) >= MAX_CLIENTS:
                print(f"🚫 Raggiunto il massimo di {MAX_CLIENTS} client.")
                break

        except socket.timeout:
            if not threads:
                print(f"⏳ Nessuna connessione ricevuta entro {CONNECTION_TIMEOUT} secondi. Server chiuso.")
                break
            else:
                continue  # Continua ad ascoltare finché almeno un client si è connesso

except KeyboardInterrupt:
    print("🛑 Interruzione manuale.")

finally:
    server.close()
    for t in threads:
        t.join()
    save_model()
    print("✅ Server terminato.")
