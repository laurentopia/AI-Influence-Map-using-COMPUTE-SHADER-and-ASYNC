Calculating a diffusion influence map using a compute shader

![](https://i.imgur.com/0uvmqx9.png)

This project attempts to calculate diffusion on a large map using the GPU, it's currently working ok on a 600x600 map.

AsyncGPUReadback is fast, 0.1ms. The 8ms spikes are due to the convertion world space to texture and can be smoothed out by moving all this code to a bursted job.
![](https://i.imgur.com/5tCdyLg.png)
