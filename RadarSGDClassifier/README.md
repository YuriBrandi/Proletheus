# 🧠 Radar Classifier – Training & Inferenza

  

Sistema di classificazione binaria (nemico / non nemico) integrato in Unity, con training in tempo reale e inferenza embedded.

  

---

  

## 🚀 Modalità Training

  

### 🎯 Obiettivo

Addestrare il radar in tempo reale utilizzando 6 mappe in parallelo, con visualizzazione su TensorBoard. 

  

### ✅ Istruzioni

  

1. **Apri Unity** e seleziona la scena `TrainingScene` (contiene 6 mappe parallele).

2. **Esegui il server Python** per il training:

   ```bash

   python online_training_server.py

   ```

3. **Avvia la scena in Unity**: il radar inizierà a classificare oggetti e apprendere dai risultati.

4. **Stoppa la scena** per terminare il training.

  

### ⚠️ Nota importante

Nel componente `SGDClassifier` (sul radar tower):

  

- **Lascia il campo `Json File` vuoto**  

  → Questo segnala al sistema che sei in modalità **training**.

  

---

  

### 📦 Output generato

  

| Percorso        | File                        | Descrizione                                      |

|----------------|-----------------------------|--------------------------------------------------|

| `models/`      | `trained_model.pkl`         | Modello addestrato in formato `joblib`          |

|                | `checkpoint.npz`            | Stato del training per riprendere successivamente |

| `runs/`        | *(log TensorBoard)*         | File di log per il monitoraggio delle metriche  |

  

---

  

### 📊 TensorBoard (visualizzazione live)

  

Per monitorare accuracy e loss:

  

```bash

tensorboard --logdir=runs/

```

  

---

  

## 🤖 Modalità Inferenza

  

### 🎯 Obiettivo

Effettuare classificazioni direttamente in Unity senza connessione a Python.

  

### ✅ Istruzioni

  

1. **Apri Unity** e seleziona la scena `MainScene`
2. **Esporta i pesi dal modello Python**:

   ```bash

   python export_weights.py

   ```

   Verrà generato:  

   `models/sgd_weights.json`

  

2. **Importa il file in Unity**:

   - Sposta `sgd_weights.json` nella cartella `Assets/ML/`

   - Assegnalo nel campo `Json File` dello script `SGDClassifier` (presente su `RadarTowerPlaceholder`)

  

   ✅ Il sistema ora capisce che sei in modalità **inferenza**.

  

3. **Avvia la scena**  

   → Il radar userà il modello addestrato per classificare in tempo reale.
