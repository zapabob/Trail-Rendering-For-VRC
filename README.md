# ArmTrailRenderer

ArmTrailRenderer is a Unity script that allows you to add a beam saber trail effect to your modular avatar's arms. The trail follows the movement of the avatar's arms and can be customized using various parameters.

## Features

* Beam saber trail effect that follows the avatar's arm movements
* Customizable trail duration, width, and color
* Integration with audio spectrum data for dynamic trail effects
* Optimized for performance on RX 5700 XT (VRAM 8GB) using CUDA cores
* Radial menu for adjusting trail parameters in real-time

## Requirements

* Unity Editor (version 2019.4 or later)
* Modular Avatar project
* RX 5700 XT (VRAM 8GB) or compatible GPU (optional, for optimized performance)

## Installation

1. Clone or download the repository to your local machine.
2. Open your Unity project and navigate to the modular avatar scene.
3. Create a new folder in the "Assets" directory and name it "Scripts".
4. Place the `ArmTrailRenderer.cs` script in the "Scripts" folder.
5. Create a new folder in the "Assets" directory and name it "Prefabs".
6. Create a new prefab in the "Prefabs" folder and name it "ArmTrailRenderer".
7. Open the prefab and add two empty game objects named "LeftLineRenderer" and "RightLineRenderer".
8. Add a Line Renderer component to each game object.
9. Save the prefab.

## Usage

1. Select the root object of your modular avatar in the Unity Editor.
2. Add the `ArmTrailRenderer` script to the avatar by clicking "Add Component" in the Inspector and searching for "ArmTrailRenderer".
3. Assign the "LeftLineRenderer" and "RightLineRenderer" game objects to the respective fields in the `ArmTrailRenderer` component.
4. Assign your desired beam saber sound clip to the "Beam Saber Sound" field.
5. Adjust the other parameters in the `ArmTrailRenderer` component according to your preferences.
6. Optimize your avatar following the guidelines of the modular avatar project.
7. Set up a radial menu to control the trail parameters using the `SetTrailDurationMultiplier()`, `SetTrailWidthMultiplier()`, and `SetFFTSizeMultiplier()` methods.

## Configuration

The `ArmTrailRenderer` script provides several parameters that you can adjust to customize the appearance and behavior of the beam saber trail:

* `Left Line Renderer`: Assign the "LeftLineRenderer" game object to this field.
* `Right Line Renderer`: Assign the "RightLineRenderer" game object to this field.
* `Min Trail Duration`: The minimum duration of the trail in seconds.
* `Max Trail Duration`: The maximum duration of the trail in seconds.
* `Min Trail Width`: The minimum width of the trail.
* `Max Trail Width`: The maximum width of the trail.
* `Default Trail Color Gradient`: The default color gradient of the trail.
* `Beam Saber Sound`: Assign your desired beam saber sound clip to this field.
* `Min Pitch`: The minimum pitch of the beam saber sound.
* `Max Pitch`: The maximum pitch of the beam saber sound.
* `FFT Size`: The size of the FFT (Fast Fourier Transform) used for audio spectrum analysis.
* `Noise Threshold`: The threshold for noise reduction in the audio spectrum.

## Optimization

The `ArmTrailRenderer` script is optimized for performance on RX 5700 XT (VRAM 8GB) using CUDA cores. However, it can also run on other GPUs, although the performance may vary.

To ensure the best performance, follow these guidelines:

* Optimize your avatar following the guidelines of the modular avatar project.
* Adjust the trail parameters to find the right balance between visual quality and performance.
* Use the radial menu to control the trail parameters in real-time and find the optimal settings for your avatar.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgments

* The modular avatar project for providing the foundation for this script.
* The Unity community for their valuable resources and support.

## Feedback and Contributions

If you have any feedback, suggestions, or would like to contribute to this project, please feel free to open an issue or submit a pull request on the GitHub repository.

