using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.CLSID;
using static TerraFX.Interop.Windows.Windows;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
unsafe class Program
{
	static void Main()
	{
		const int buffLen = 128;
		byte* buff = stackalloc byte[buffLen];

		// 32x32 YCCK JPEG
		using var jpeg = (UnmanagedMemoryStream)typeof(Program).Assembly.GetManifestResourceStream("wic-jpeg-bug.cmyk.jpg")!;

		using var factory = default(ComPtr<IWICImagingFactory>);
		CoCreateInstance((Guid*)Unsafe.AsPointer(ref Unsafe.AsRef(in CLSID_WICImagingFactory)), null, (uint)CLSCTX.CLSCTX_INPROC_SERVER, __uuidof<IWICImagingFactory>(), (void**)factory.GetAddressOf()).Assert();

		using var stm = default(ComPtr<IWICStream>);
		factory.Get()->CreateStream(stm.GetAddressOf()).Assert();
		stm.Get()->InitializeFromMemory(jpeg.PositionPointer, (uint)jpeg.Length).Assert();

		using var decoder = default(ComPtr<IWICBitmapDecoder>);
		factory.Get()->CreateDecoderFromStream((IStream*)stm.Get(), null, WICDecodeOptions.WICDecodeMetadataCacheOnDemand, decoder.GetAddressOf()).Assert();

		using var frame = default(ComPtr<IWICBitmapFrameDecode>);
		decoder.Get()->GetFrame(0, frame.GetAddressOf()).Assert();

		// reading full image width returns correct results
		var rect = new WICRect { Width = 32, Height = 1 };

		frame.Get()->CopyPixels(&rect, buffLen, buffLen, buff).Assert();
		Console.WriteLine($"First pixel: 0x{*(uint*)buff:x8}");

		// reading partial width returns inverted values
		rect.Width = 16;

		frame.Get()->CopyPixels(&rect, buffLen, buffLen, buff).Assert();
		Console.WriteLine($"First pixel: 0x{*(uint*)buff:x8}");
	}
}

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
static class HResultExtensions
{
	public static void Assert(this HRESULT hr)
	{
		if (hr.FAILED)
			Marshal.ThrowExceptionForHR(hr);
	}
}
