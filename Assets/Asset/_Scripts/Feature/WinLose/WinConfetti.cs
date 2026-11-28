using Cysharp.Threading.Tasks;
using UnityEngine;

internal static class WinConfetti
{
    private static readonly Color32[] Colors =
    {
        new Color32(255, 44, 71, 255), new Color32(255, 158, 23, 255),
        new Color32(255, 235, 45, 255), new Color32(94, 246, 44, 255),
        new Color32(0, 230, 218, 255), new Color32(43, 151, 255, 255)
    };

    internal static async UniTask Play(Camera camera)
    {
        if (!camera) return;
        float depth = camera.nearClipPlane + 0.5f;
        Vector3 center = camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));
        Vector3 corner = camera.transform.InverseTransformPoint(camera.ViewportToWorldPoint(new Vector3(1, 1, depth)));
        var emitter = new GameObject("Win Confetti");
        emitter.transform.SetParent(camera.transform, false);
        emitter.transform.position = center;

        var particles = emitter.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.loop = false;
        main.useUnscaledTime = true;
        main.startSize3D = true;
        main.startRotation3D = true;
        main.maxParticles = 180;
        var emission = particles.emission;
        emission.enabled = false;
        var shape = particles.shape;
        shape.enabled = false;
        var spin = particles.rotationOverLifetime;
        spin.enabled = true;
        spin.separateAxes = true;
        spin.x = new ParticleSystem.MinMaxCurve(3f, 9f);
        spin.y = new ParticleSystem.MinMaxCurve(-5f, 5f);
        spin.z = new ParticleSystem.MinMaxCurve(-4f, 4f);
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = Resources.Load<Material>("WinConfetti");
        renderer.sortingOrder = 100;
        particles.Play();
        for (int i = 0; i < main.maxParticles; i++)
        {
            float length = Random.Range(0.0468f, 0.0918f) * corner.x;
            var piece = new ParticleSystem.EmitParams
            {
                position = new Vector3(Random.Range(-1.07f, 1.07f) * corner.x, Random.Range(0.82f, 1.95f) * corner.y, 0),
                velocity = new Vector3(Random.Range(-0.24f, 0.24f) * corner.x, -Random.Range(1.3f, 1.75f) * corner.y, 0),
                startLifetime = 2.35f,
                startSize3D = new Vector3(length, length * Random.Range(0.336f, 0.6f), length),
                rotation3D = new Vector3(0, 0, Random.Range(0f, 180f)),
                startColor = Colors[Random.Range(0, Colors.Length)]
            };
            particles.Emit(piece, 1);
        }
        try
        {
            await UniTask.Delay(2350, ignoreTimeScale: true, cancellationToken: emitter.GetCancellationTokenOnDestroy());
        }
        finally
        {
            if (emitter) Object.Destroy(emitter);
        }
    }
}
