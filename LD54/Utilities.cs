using Stride.Core.Mathematics;
using Stride.Rendering.Materials.ComputeColors;
using Stride.Rendering.Materials;
using Stride.Rendering;
using Stride.Engine;
using Stride.Physics;
using Stride.Rendering.ProceduralModels;
using Stride.Games;
using Stride.UI.Panels;
using Stride.UI;
using Stride.Core.Diagnostics;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;

static class Utilities
{
    public static Material GetDiffuseMaterial(Color color)
    {
        var materialDescription = new MaterialDescriptor
        {
            Attributes =
                {
                    Diffuse = new MaterialDiffuseMapFeature(new ComputeColor(color)),
                    DiffuseModel = new MaterialDiffuseLambertModelFeature()
                }
        };

        return Material.New(Program.Game.GraphicsDevice, materialDescription);
    }

    #region Stride Community Toolkit
    ///see https://github.com/VaclavElias/stride-community-toolkit

    /// <summary>
    /// Gets the time elapsed since the last game update in seconds as a single-precision floating-point number.
    /// </summary>
    /// <param name="gameTime">The IGame interface providing access to game timing information.</param>
    /// <returns>The time elapsed since the last game update in seconds.</returns>
    public static float DeltaTime(this IGame gameTime)
    {
        return (float)gameTime.UpdateTime.Elapsed.TotalSeconds;
    }

    public static Grid CreateGrid(params UIElement[] elements)
    {
        var grid = new Grid();

        grid.Children.AddRange(elements);

        return grid;
    }

    // Duplicated from Stride.Assets.Presentation.Preview
    public enum PrimitiveModelType
    {
        Sphere,
        Cube,
        Cylinder,
        Torus,
        Plane,
        Teapot,
        Cone,
        Capsule
    }

    /// <summary>
    /// Creates an entity with a primitive procedural model with a primitive mesh renderer and adds appropriate collider except for Torus, Teapot and Plane.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="type"></param>
    /// <param name="entityName"></param>
    /// <param name="material"></param>
    /// <param name="includeCollider">Adds a default collider except for Torus, Teapot and Plane. Default true.</param>
    /// <returns></returns>
    public static Entity CreatePrimitive(this Game game, PrimitiveModelType type, string entityName = null, Material material = null, bool includeCollider = true)
    {
        var proceduralModel = getProceduralModel(type);

        var model = proceduralModel.Generate(game.Services);

        model.Materials.Add(material);

        var entity = new Entity(entityName) { new ModelComponent(model) };

        if (!includeCollider) return entity;

        var colliderShape = getColliderShape(type);

        if (colliderShape is null) return entity;

        var collider = new RigidbodyComponent();

        collider.ColliderShapes.Add(colliderShape);

        entity.Add(collider);

        return entity;
    }

    private static PrimitiveProceduralModelBase getProceduralModel(PrimitiveModelType type)
        => type switch
        {
            PrimitiveModelType.Plane => new PlaneProceduralModel(),
            PrimitiveModelType.Sphere => new SphereProceduralModel(),
            PrimitiveModelType.Cube => new CubeProceduralModel(),
            PrimitiveModelType.Cylinder => new CylinderProceduralModel(),
            PrimitiveModelType.Torus => new TorusProceduralModel(),
            PrimitiveModelType.Teapot => new TeapotProceduralModel(),
            PrimitiveModelType.Cone => new ConeProceduralModel(),
            PrimitiveModelType.Capsule => new CapsuleProceduralModel(),
            _ => throw new InvalidOperationException(),
        };

    private static IInlineColliderShapeDesc getColliderShape(PrimitiveModelType type)
        => type switch
        {
            PrimitiveModelType.Plane => null,
            PrimitiveModelType.Sphere => new SphereColliderShapeDesc(),
            PrimitiveModelType.Cube => new BoxColliderShapeDesc(),
            PrimitiveModelType.Cylinder => new CylinderColliderShapeDesc(),
            PrimitiveModelType.Torus => null,
            PrimitiveModelType.Teapot => null,
            PrimitiveModelType.Cone => new ConeColliderShapeDesc(),
            PrimitiveModelType.Capsule => new CapsuleColliderShapeDesc(),
            _ => throw new InvalidOperationException(),
        };

    public class DebugLogger : Logger
    {
        public static DebugLogger Instance = new();

        protected override void LogRaw(ILogMessage logMessage)
        {
            System.Diagnostics.Debug.WriteLine(logMessage.Text);
        }
    }
    #endregion

    #region Audio
    //see https://markheath.net/post/fire-and-forget-audio-playback-with
    public class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        public void PlaySound(string fileName)
        {
            var input = new AudioFileReader(fileName);
            AddMixerInput(new AutoDisposeFileReader(input));
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
    }
    public class CachedSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                // TODO: could add resampling in here if required
                WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }

        public void Play()
        {
            AudioPlaybackEngine.Instance.PlaySound(this);
        }
    }
    public class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        private long position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }
    public class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
    #endregion
}
