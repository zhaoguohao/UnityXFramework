using System.Collections;
using System.IO;
using System;

public class SpPacker {
	private static int PackSeg (byte[] src, int src_offset, SpStream output, int n) {
        byte header = 0;
        int notzero = 0;

		int dest_begin = output.Position;
		output.Write((byte)0);

        for (int i = 0; i < 8; i++) {
            if (src[src_offset + i] != 0) {
                notzero++;
                header |= (byte)(1 << i);

				output.Write (src[src_offset + i]);
            }
        }

        if ((notzero == 7 || notzero == 6) && n > 0)
            notzero = 8;

        if (notzero == 8) {
            if (n > 0)
                return 8;
            else
                return 10;
        }

		int dest_end = output.Position;
		output.Position = dest_begin;
		output.Write (header);
		output.Position = dest_end;

        return notzero + 1;
    }

	public static void WriteFF (byte[] src, int src_offset, SpStream output, int n) {
		output.Write ((byte)0xff);
		output.Write ((byte)(n - 1));
		output.Write (src, src_offset, n * 8);
    }

	public static SpStream Pack (SpStream input) {
		SpStream stream = new SpStream ();

		int pos = input.Position;
		if (Pack (input, stream) == false) {
			int size = stream.Position;
			size = ((size + 7) / 8) * 8;
			stream = new SpStream (size);

			input.Position = pos;
			// do we need this check? it's not possible it will return false this time
			if (Pack (input, stream) == false)
				return null;
		}
		
		return stream;
	}

    public static bool Pack (SpStream input, SpStream output) {
		int src_size = input.Length;
		if (src_size % 8 != 0) {
			int new_size = ((src_size + 7) / 8) * 8;
			if (input.Capacity < new_size) {
				SpStream new_input = new SpStream (new_size);
				Array.Copy (input.Buffer, input.Offset, new_input.Buffer, new_input.Offset, input.Length);
				new_input.Position = new_input.Offset;
				input = new_input;
			}
			else {
				int pos = input.Position;
				input.Position = input.Tail;
				for (int i = src_size; i < new_size; i++) {
					input.Write ((byte)0);
				}
				input.Position = pos;
			}
		}

		int src_offset = input.Offset;
		byte[] src = input.Buffer;

        int ff_n = 0;
        int ff_src_start = 0;
        int ff_dest_start = 0;

        for (; src_offset < src_size; src_offset += 8) {
			int pos = output.Position;
			int n = PackSeg (src, src_offset, output, ff_n);

            if (n == 10) {
                ff_src_start = src_offset;
				ff_dest_start = pos;
                ff_n = 1;
            }
            else if (n == 8 && ff_n > 0) {
                ff_n++;
                if (ff_n == 256) {
					output.Position = ff_dest_start;
                    WriteFF (src, ff_src_start, output, 256);
                    ff_n = 0;
                }
            }
            else {
				if (ff_n > 0) {
					output.Position = ff_dest_start;
                    WriteFF (src, ff_src_start, output, ff_n);
                    ff_n = 0;
                }
            }

			output.Position = pos + n;
        }

		if (ff_n > 0) {
			output.Position = ff_dest_start;
			WriteFF (src, ff_src_start, output, ff_n);
            output.CutTailToPosition();
		}

		return (output.IsOverflow () == false);
    }

	public static SpStream Unpack (SpStream input) {
        SpStream stream = SpStreamCache.Get(); // new SpStream ();
		
		int pos = input.Position;
		if (Unpack (input, stream) == false) {
			stream = new SpStream (stream.Position);
			
			input.Position = pos;
			if (Unpack (input, stream) == false)
				return null;
		}
		
		return stream;
	}

    public static bool Unpack (SpStream input, SpStream output) {
		int src_size = input.Tail;
		int src_offset = input.Offset;
		byte[] src = input.Buffer;

		while (src_offset < src_size) {
			byte header = src[src_offset];
			src_offset++;

			if (header == 0xff) {
				if (src_offset >= src_size)
					return false;

				int n = (src[src_offset] + 1) * 8;
				if (src_size - src_offset < n + 1)
					return false;

				src_offset++;
				output.Write (src, src_offset, n);
				src_offset += n;
			}
			else {
				for (int i = 0; i < 8; i++) {
					int nz = (header >> i) & 1;
					if (nz != 0) {
						if (src_offset >= src_size)
							return false;

						output.Write (src[src_offset]);
						src_offset++;
					}
					else {
						output.Write ((byte)0);
					}
				}
			}
		}
		
		return (output.IsOverflow () == false);
    }
}
