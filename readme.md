This is a standalone reproduction for a bug in WIC's JPEG decoder.  When reading less than a full image row from a CMYK JPEG, pixel values are returned inverted.

On WindowsCodecs.dll 10.0.26100.2134 it prints:

```
First pixel: 0x00000000
First pixel: 0xffffffff
```

On WindowsCodecs.dll 10.0.22621.4455 it prints:

```
First pixel: 0x00000000
First pixel: 0x00000000
```
