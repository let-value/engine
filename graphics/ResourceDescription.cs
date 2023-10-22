//using Vortice.Direct3D12;
//using Vortice.DXGI;

//namespace graphics;

//public record struct ResourceDescriptionaaa
//{
//    public ResourceDimension Dimension { get; init; }

//    public long Alignment { get; init; }

//    public long Width { get; init; }

//    public int Height { get; init; }

//    public short DepthOrArraySize { get; init; }

//    public short MipLevels { get; init; }

//    public Format Format { get; init; }

//    public SampleDescription SampleDescription { get; init; }

//    public TextureLayout Layout { get; init; }

//    public ResourceFlags Flags { get; init; }

//    public static ResourceDescription Buffer(int sizeInBytes, ResourceFlags bufferFlags = ResourceFlags.None)
//    {
//        return new()
//        {
//            Dimension = ResourceDimension.Buffer,
//            Width = sizeInBytes,
//            Height = 1,
//            DepthOrArraySize = 1,
//            MipLevels = 1,
//            SampleDescription = new(1, 0),
//            Layout = TextureLayout.RowMajor,
//            Flags = bufferFlags,
//        };
//    }

//    public static ResourceDescription Texture2D(
//        int width,
//        int height,
//        Format format,
//        ResourceFlags textureFlags = ResourceFlags.None,
//        short mipLevels = 1,
//        short arraySize = 1,
//        int sampleCount = 1,
//        int sampleQuality = 0
//    )
//    {
//        return new()
//        {
//            Dimension = ResourceDimension.Texture2D,
//            Width = width,
//            Height = height,
//            DepthOrArraySize = arraySize,
//            MipLevels = mipLevels,
//            Format = format,
//            SampleDescription = new(sampleCount, sampleQuality),
//            Flags = textureFlags,
//        };
//    }

//    public static implicit operator ResourceDescription(ImageDescription description)
//    {
//        return new()
//        {
//            Dimension = description.Dimension,
//            Width = description.Width,
//            Height = description.Height,
//            DepthOrArraySize = description.DepthOrArraySize,
//            MipLevels = description.MipLevels,
//            Format = description.Format,
//            SampleDescription = new(1, 0),
//            Flags = ResourceFlags.None,
//        };
//    }

//    public static implicit operator ImageDescription(ResourceDescription description)
//    {
//        return new()
//        {
//            Dimension = description.Dimension,
//            Width = (int)description.Width,
//            Height = description.Height,
//            DepthOrArraySize = description.DepthOrArraySize,
//            MipLevels = description.MipLevels,
//            Format = description.Format,
//        };
//    }
//}

