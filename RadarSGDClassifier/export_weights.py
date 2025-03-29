import joblib
import json
import numpy as np

model = joblib.load("models/trained_model.pkl")

weights = model.coef_.flatten().tolist()    # shape: (1, N)
bias = model.intercept_[0]                  # singolo valore

data = {
    "weights": weights,
    "bias": bias
}

with open("models/sgd_weights.json", "w") as f:
    json.dump(data, f, indent=4)

print("✅ Pesi esportati in models/sgd_weights.json")
