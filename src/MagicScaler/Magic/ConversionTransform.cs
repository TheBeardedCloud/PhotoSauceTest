// Copyright © Clinton Ingram and Contributors.  Licensed under the MIT License.

using System;

using PhotoSauce.MagicScaler.Converters;

namespace PhotoSauce.MagicScaler.Transforms
{
	internal sealed class ConversionTransform : ChainedPixelSource
	{
		private readonly IConversionProcessor processor;

		public override PixelFormat Format { get; }

		public ConversionTransform(PixelSource source, PixelFormat destFormat, ColorProfile? sourceProfile = null, ColorProfile? destProfile = null) : base(source)
		{
			var srcFormat = source.Format;
			Format = destFormat;

			if (srcFormat.ColorRepresentation == PixelColorRepresentation.Cmyk && srcFormat.BitsPerPixel == 64 && Format.BitsPerPixel == 32)
			{
				processor = NarrowingConverter.Instance;
			}
			else if (srcFormat.Range == PixelValueRange.Video && Format.Range == PixelValueRange.Full && srcFormat.BitsPerPixel == 8 && Format.BitsPerPixel == 8)
			{
				processor = srcFormat.Encoding == PixelValueEncoding.Chroma ? VideoChromaConverter.VideoToFullRangeProcessor.Instance : VideoLumaConverter.VideoToFullRangeProcessor.Instance;
			}
			else if (srcFormat.Encoding == PixelValueEncoding.Companded && Format.Encoding == PixelValueEncoding.Linear)
			{
				var srcProfile = sourceProfile as CurveProfile ?? ColorProfile.sRGB;
				if (Format.NumericRepresentation == PixelNumericRepresentation.Fixed)
					if (srcFormat.AlphaRepresentation != PixelAlphaRepresentation.None)
						processor = srcProfile.GetConverter<byte, ushort, EncodingType.Companded>().Processor3A;
					else if (srcFormat.Range == PixelValueRange.Video)
						processor = srcProfile.GetConverter<byte, ushort, EncodingType.Companded, EncodingRange.Video>().Processor;
					else
						processor = srcProfile.GetConverter<byte, ushort, EncodingType.Companded>().Processor;
				else if (srcFormat.NumericRepresentation == PixelNumericRepresentation.UnsignedInteger && Format.NumericRepresentation == PixelNumericRepresentation.Float)
					if (srcFormat.AlphaRepresentation != PixelAlphaRepresentation.None)
						processor = srcProfile.GetConverter<byte, float, EncodingType.Companded>().Processor3A;
					else if (srcFormat.ChannelCount == 3 && Format.ChannelCount == 4)
						processor = srcProfile.GetConverter<byte, float, EncodingType.Companded>().Processor3X;
					else if (srcFormat.Range == PixelValueRange.Video)
						processor = srcProfile.GetConverter<byte, float, EncodingType.Companded, EncodingRange.Video>().Processor;
					else
						processor = srcProfile.GetConverter<byte, float, EncodingType.Companded>().Processor;
				else if (Format.NumericRepresentation == PixelNumericRepresentation.Float)
					if (srcFormat.AlphaRepresentation != PixelAlphaRepresentation.None)
						processor = srcProfile.GetConverter<float, float, EncodingType.Companded>().Processor3A;
					else
						processor = srcProfile.GetConverter<float, float, EncodingType.Companded>().Processor;
			}
			else if (srcFormat.Encoding == PixelValueEncoding.Linear && Format.Encoding == PixelValueEncoding.Companded)
			{
				var dstProfile = destProfile as CurveProfile ?? ColorProfile.sRGB;
				if (srcFormat.NumericRepresentation == PixelNumericRepresentation.Fixed)
					if (srcFormat.AlphaRepresentation != PixelAlphaRepresentation.None)
						processor = dstProfile.GetConverter<ushort, byte, EncodingType.Linear>().Processor3A;
					else
						processor = dstProfile.GetConverter<ushort, byte, EncodingType.Linear>().Processor;
				else if (srcFormat.NumericRepresentation == PixelNumericRepresentation.Float && Format.NumericRepresentation == PixelNumericRepresentation.UnsignedInteger)
					if (srcFormat.AlphaRepresentation != PixelAlphaRepresentation.None)
						processor = dstProfile.GetConverter<float, byte, EncodingType.Linear>().Processor3A;
					else if (srcFormat.ChannelCount == 4 && Format.ChannelCount == 3)
						processor = dstProfile.GetConverter<float, byte, EncodingType.Linear>().Processor3X;
					else
						processor = dstProfile.GetConverter<float, byte, EncodingType.Linear>().Processor;
				else if (srcFormat.NumericRepresentation == PixelNumericRepresentation.Float)
					if (srcFormat.AlphaRepresentation != PixelAlphaRepresentation.None)
						processor = dstProfile.GetConverter<float, float, EncodingType.Linear>().Processor3A;
					else
						processor = dstProfile.GetConverter<float, float, EncodingType.Linear>().Processor;
			}
			else if (srcFormat.NumericRepresentation == PixelNumericRepresentation.UnsignedInteger && Format.NumericRepresentation == PixelNumericRepresentation.Float)
			{
				if (srcFormat.AlphaRepresentation == PixelAlphaRepresentation.Unassociated)
					processor = FloatConverter.Widening.InstanceFullRange.Processor3A;
				else if (srcFormat.AlphaRepresentation == PixelAlphaRepresentation.None && srcFormat.ChannelCount == 3 && Format.ChannelCount == 4)
					processor = FloatConverter.Widening.InstanceFullRange.Processor3X;
				else if (srcFormat.Encoding == PixelValueEncoding.Chroma)
					processor = srcFormat.Range == PixelValueRange.Full ? FloatConverter.Widening.InstanceFullChroma.Processor : FloatConverter.Widening.InstanceVideoChroma.Processor;
				else
					processor = srcFormat.Range == PixelValueRange.Full ? FloatConverter.Widening.InstanceFullRange.Processor : FloatConverter.Widening.InstanceVideoRange.Processor;
			}
			else if (srcFormat.NumericRepresentation == PixelNumericRepresentation.Float && Format.NumericRepresentation == PixelNumericRepresentation.UnsignedInteger)
			{
				if (Format.AlphaRepresentation != PixelAlphaRepresentation.None)
					processor = FloatConverter.Narrowing.InstanceFullRange.Processor3A;
				else if (srcFormat.AlphaRepresentation == PixelAlphaRepresentation.None && srcFormat.ChannelCount == 4 && Format.ChannelCount == 3)
					processor = FloatConverter.Narrowing.InstanceFullRange.Processor3X;
				else if (Format.Encoding == PixelValueEncoding.Chroma)
					processor = Format.Range == PixelValueRange.Full ? FloatConverter.Narrowing.InstanceFullChroma.Processor : FloatConverter.Narrowing.InstanceVideoChroma.Processor;
				else
					processor = Format.Range == PixelValueRange.Full ? FloatConverter.Narrowing.InstanceFullRange.Processor : FloatConverter.Narrowing.InstanceVideoRange.Processor;
			}
			else if (srcFormat.NumericRepresentation == Format.NumericRepresentation && srcFormat.ChannelCount != Format.ChannelCount)
			{
				if (srcFormat.NumericRepresentation == PixelNumericRepresentation.Float)
					processor = ChannelChanger<float>.GetConverter(srcFormat.ChannelCount, Format.ChannelCount);
				else if (srcFormat.NumericRepresentation == PixelNumericRepresentation.Fixed)
					processor = ChannelChanger<ushort>.GetConverter(srcFormat.ChannelCount, Format.ChannelCount);
				else
					processor = ChannelChanger<byte>.GetConverter(srcFormat.ChannelCount, Format.ChannelCount);
			}
			else if (srcFormat.NumericRepresentation == Format.NumericRepresentation && srcFormat.ChannelCount == Format.ChannelCount && srcFormat.ColorRepresentation != Format.ColorRepresentation)
			{
				if (srcFormat.NumericRepresentation == PixelNumericRepresentation.Float)
					processor = ChannelChanger<float>.GetConverter(srcFormat.ChannelCount, Format.ChannelCount);
				else if (srcFormat.NumericRepresentation == PixelNumericRepresentation.Fixed)
					processor = ChannelChanger<ushort>.GetConverter(srcFormat.ChannelCount, Format.ChannelCount);
				else
					processor = ChannelChanger<byte>.GetConverter(srcFormat.ChannelCount, Format.ChannelCount);
			}

			if (processor is null)
				throw new NotSupportedException("Unsupported pixel format");
		}

		protected override unsafe void CopyPixelsInternal(in PixelArea prc, int cbStride, int cbBufferSize, byte* pbBuffer)
		{
			if (PrevSource.Format.BitsPerPixel > Format.BitsPerPixel)
				copyPixelsBuffered(prc, cbStride, pbBuffer);
			else
				copyPixelsDirect(prc, cbStride, pbBuffer);
		}

		private unsafe void copyPixelsBuffered(in PixelArea prc, int cbStride, byte* pbBuffer)
		{
			int buffStride = BufferStride;
			using var buff = BufferPool.RentLocalAligned<byte>(buffStride);

			fixed (byte* bstart = buff.Span)
			{
				int cb = MathUtil.DivCeiling(prc.Width * PrevSource.Format.BitsPerPixel, 8);

				for (int y = 0; y < prc.Height; y++)
				{
					Profiler.PauseTiming();
					PrevSource.CopyPixels(new PixelArea(prc.X, prc.Y + y, prc.Width, 1), buffStride, buffStride, bstart);
					Profiler.ResumeTiming();

					byte* op = pbBuffer + y * cbStride;
					processor.ConvertLine(bstart, op, cb);
				}
			}
		}

		private unsafe void copyPixelsDirect(in PixelArea prc, int cbStride, byte* pbBuffer)
		{
			int cbi = MathUtil.DivCeiling(prc.Width * PrevSource.Format.BitsPerPixel, 8);
			int cbo = MathUtil.DivCeiling(prc.Width * Format.BitsPerPixel, 8);

			for (int y = 0; y < prc.Height; y++)
			{
				byte* op = pbBuffer + y * cbStride;
				byte* ip = op + cbo - cbi;

				Profiler.PauseTiming();
				PrevSource.CopyPixels(new PixelArea(prc.X, prc.Y + y, prc.Width, 1), cbStride, cbi, ip);
				Profiler.ResumeTiming();

				processor.ConvertLine(ip, op, cbi);
			}
		}

		public override string ToString() => $"{nameof(ConversionTransform)}: {PrevSource.Format.Name}->{Format.Name}";
	}

	/// <summary>Converts an image to an alternate pixel format.</summary>
	public sealed class FormatConversionTransform : PixelTransformInternalBase
	{
		private readonly Guid outFormat;

		/// <summary>Constructs a new <see cref="FormatConversionTransform" /> using the specified <paramref name="outFormat" />.</summary>
		/// <param name="outFormat">The desired output format.  Must be a member of <see cref="PixelFormats" />.</param>
		public FormatConversionTransform(Guid outFormat)
		{
			if (outFormat != PixelFormats.Grey8bpp && outFormat != PixelFormats.Bgr24bpp && outFormat != PixelFormats.Bgra32bpp)
				throw new NotSupportedException("Unsupported pixel format");

			this.outFormat = outFormat;
		}

		internal override void Init(PipelineContext ctx)
		{
			MagicTransforms.AddExternalFormatConverter(ctx);

			if (ctx.Source.Format.FormatGuid != outFormat)
				ctx.Source = ctx.AddProfiler(new ConversionTransform(ctx.Source, PixelFormat.FromGuid(outFormat)));

			Source = ctx.Source;
		}
	}
}
