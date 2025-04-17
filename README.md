# Multi-ML-agent 3D Missile defence simulation

<p align='center'> 
    <img src=md_images/screenshot.png width=900>
</p>


This University work consists of a sophisticated Multi-ML-agent setup composed of:

1. A minimal Custom-made architecture for a **Stochastic Gradient Descent (SGD) Classifier**.
2. A native ML-agent curriculum-based **Proximal policy optimization (PPO)** model.

The first model allows for *Threat Objects* (i.e. missiles) to be discerned from friendly benign objects (i.e. aircrafts, birds etc.).

Once classified, the *Threat Objects* are transmitted to grid of military interceptors. The nearest one will then launch a "smart" missile powered by the *PPO model* that will try to intercept the targeted object as soon as possible.

## Authors
- [@YuriBrandi](https://github.com/YuriBrandi)
- [@GerardoLeone](https://github.com/GerardoLeone)
- [@DomenicoAnzalone](https://github.com/DomenicoAnzalone)

## Environment and Architecture

In order to have a lighter environment *(URP)* to train our objects in, we made a separate branch for training https://github.com/YuriBrandi/Proletheus/tree/training.

In this branch you can find every environment set up ready for each phase of the training.

### Inference

<p align='left'> 
    <img src=md_images/InferenceScheme1.png width=500><img src=md_images/InferenceScheme2.png width=500>
    <img src=md_images/InferenceScheme3.png width=500>
</p>

This is what the final setup does. Whenever an object enters the *Radar* radius, it is permanently classified as either ***1** (enemy)* or ***2** (friendly)* using the trained *SGDClassifier*.

When an object is classified as *enemy* it is transmitted by the Radar to the grid of Interceptors, of which the nearest one sends a *PPO-based* defence missile.

<p align='left'> 
    <img src=md_images/inference.GIF width=600>
</p>


### PPO Training

<p align='left'> 
    <img src=md_images/TrainingScheme1.png width=500>
</p>

When the *defence missile* gets farther the target object, it receives a negative reward, else a positive one. This behaviour encourages fast stall-free interceptions.

<p align='left'> 
    <img src=md_images/TrainingScheme2.png width=500>
</p>

If the *raycasts* attached to the *defence missile* intersect with a non-target object, then the defence missile receives a negative reward to make it avoid hitting it.

<p align='left'> 
    <img src=md_images/curriculum_cumulative_reward.png width=800>
</p>

The curriculum training strategy has proven to be the right approach for this training. Allowing the model to reach an accuracy **> 95%**.

Every change in curriculum lessons happens whenever the cumulative rewards thresholds defined in the *YAML* are exceeded. As seen in the image this usually results in an immediate drop in the cumulative reward but helps the agent to learn by difficulty steps.

There are 6 lessons overall:
1. Enemy Missiles come from North only
2. E.M. come from North + South
3. E.M. come from North + South + East
4. E.M. come from North + South + East + West
5. E.M. come from Everywhere (Angles included)
6. Enabling friendly Flying Objects spawn (Aircrafts and Birds).

### Friendly Objects

Friendly objects consist of a series of Flying Objects needed to make both the classification and the interception non-trivial.

<p align='left'> 
    <img src=md_images/airbus.png width=500>
    <img src=md_images/flock.png width=500>
</p>

### Architecture

Since there are two models to be trained it is important to have two distinct phases to overcome the interdependence of the two trainings. In fact the Classifier (Radar) needs to be able to classify Defence Missiles which are only launched when the Radar finds an Enemy Missile *(loop!)*.

#### Phase 1
<p align='left'> 
    <img src=md_images/TrainingArchitecture1.png width=500>
</p>

The *PHASE 1* trains the Defence Missiles using *PPO*. To avoid the interdependence mentioned before, the Radar Classifier is *"deterministic"*, so that *`output = expected output`*.

#### Phase 2
<p align='left'> 
    <img src=md_images/TrainingArchitecture2.png width=500>
</p>

The *PHASE 2* trains the ML-Radar *(green one)* exclusively. The Defence Missiles will now work in *inference mode*.

Note that a *deterministic* Classifier is still needed to make the Defence Missiles work as expected while training the ML-Radar. Thus the training Radar does not collaborate, and only will during inference.

Since previous attempts at using *PPO* to train a Classifier had failed miserably, we had to reinvent the wheel and create a custom setup for training an SGDClassifier using *scikit-learn*.

This step produces a *Pickle* model which is incompatible with [Sentis](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Sentis.md) model interpretation. Attempts at translating a *Pickle* to an *ONNX* in a format accepted by Sentis have also failed.

The solution is to simply export the weights of the Pickle model to a *JSON* file. 

Inference is done by explicitly executing the sigmoid function using the weights, bias and prediction features.

## Requirements
- Unity 6 *(tested on 6000.0.46f1)*
- ML-Agents *(tested on 3.0.0)*
- Python3
- scikit-learn *(tested on 1.6.x)*

For the SGDClassifier training please refer to this guide https://github.com/YuriBrandi/Proletheus/tree/training/RadarSGDClassifier.


## Credits
- Models
    - [@GerardoLeone](https://github.com/GerardoLeone)
    - [Boston Map](https://skfb.ly/p7Z7X)
    - [Airbus-A380](https://skfb.ly/ps7oC)
    - [Sukhoi Su-57](https://skfb.ly/oJECV) 
    - [FATEH 110](https://skfb.ly/oPv9J)
    - [EC 135](https://skfb.ly/67ZY7)
    - [CVN-65](https://skfb.ly/ptJHO)
    - [MIM-104](https://skfb.ly/ptDJE)
- [Unity-GPU-Boids](https://github.com/Shinao/Unity-GPU-Boids)

## Contributions 

Contributions are very much appreciated. Please well describe your changes inside your PR to make it easier to understand them.

If you encounter any problem or bug that is unrelated with your own machine, please report it and open a new issue with replicable steps.

## License

This project, this readme and the drawn schemes are distributed under the [GNU General Public License v3](LICENSE).

![GPLv3Logo](https://www.gnu.org/graphics/gplv3-127x51.png)
