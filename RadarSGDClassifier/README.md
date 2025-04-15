# Radar Classifier – Training & Inference

A real-time binary classification system (enemy / non-enemy) integrated into Unity, featuring live training and embedded inference.

---

## Training Mode

### Objective

Train the radar in real time using 6 parallel maps within Unity, with live metric tracking through TensorBoard.

### Steps

1. **Open Unity** and select the `TrainingScene`, which contains six synchronized maps.
2. **Start the Python training server** by running:
   ```bash
   python online_training_server.py
   ```
3. **Run the scene in Unity**: the radar will begin classifying objects and learning from the results.
4. **Stop the scene** to end the training session.

### Important

In the `SGDClassifier` component on the Radar Tower:

- Leave the `Json File` field **empty**.  
  → This signals the system to enter **training mode**.

### Output

| Path        | File                    | Description                                  |
|-------------|-------------------------|----------------------------------------------|
| `models/`   | `trained_model.pkl`     | Trained model saved with joblib              |
|             | `checkpoint.npz`        | Training state for resume capability         |
| `runs/`     | *(TensorBoard logs)*    | Metric logs for real-time visualization      |

### Monitor Progress

To visualize accuracy and loss during training:

```bash
tensorboard --logdir=runs/
```

---

## Inference Mode

### Objective

Run radar classification directly in Unity with no Python dependency.

### Steps

1. **Open Unity** and select the `MainScene`.
2. **Export the model weights** from Python:
   ```bash
   python export_weights.py
   ```
   This will generate:  
   `models/sgd_weights.json`

3. **Import the weights into Unity**:
   - Move `sgd_weights.json` into the `Assets/ML/` folder.
   - Assign it to the `Json File` field in the `SGDClassifier` script on the `RadarTowerPlaceholder` object.

   This activates **inference mode**.

4. **Run the scene**  
   → The radar will now classify objects in real time using the trained model.