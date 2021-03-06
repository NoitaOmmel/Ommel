﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ModTheGungeon;

/*
 * Part of WakExtractor adapted from Unicorn by SaphireLattice
 * 
 * Copyright (c) 2019, Saphire Lattice
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify,
 * merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies
 * or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace Ommel {
    public class WakExtractor {
        public static Logger Logger = new Logger("WakExtractor");
        static readonly byte[] AESKey = NollaPrng.Get16Seeded(0);
        public static readonly byte[] PreUnencryptedSHA256 = {
            0xb3, 0x84,
            0x37, 0xe1, 0x5c, 0x6a, 0x73, 0x50, 0x2a, 0x59, 0x7d, 0x12,
            0xc6, 0x4a, 0xc9, 0x94, 0xee, 0xdc, 0xdb, 0xca, 0x0c, 0xd3,
            0x51, 0xd1, 0x1f, 0x3f, 0xf9, 0xbe, 0x3a, 0xc2, 0x2e, 0xa2
        };

        public bool Decrypt = false;

        public WakExtractor(bool decrypt) {
            Decrypt = decrypt;
        }

        public void EnableDecryptIfOld(string wak_path) {
            var bytes = File.ReadAllBytes(wak_path);
            byte[] hash = null;
            using (var sha256 = new SHA256Managed()) {
                hash = sha256.ComputeHash(bytes);
            }

            var match = true;
            for (var i = 0; i < hash.Length; i++) {
                if (hash[i] != PreUnencryptedSHA256[i]) {
                    match = false;
                    break;
                }
            }

            if (match) {
                Logger.Debug($"Decryption force-enabled (main branch data.wak)");
                Decrypt = true;
            } else {
                Logger.Debug($"Decryption NOT force-enabled");
            }
        }

        public List<string> Extract(string wak_path, string target_dir) {
            var files = new List<string>();

            using (var stream = File.OpenRead(wak_path)) {
                var header = DecryptStream(stream, 0x10, 1);
                var numFiles = BitConverter.ToInt32(header, 4);
                var tocSize = BitConverter.ToInt32(header, 8);

                var tocBytes = DecryptStream(stream, tocSize - 0x10, int.MaxValue - 1);
                using (var tocStream = new MemoryStream(tocBytes))
                using (var reader = new BinaryReader(tocStream)) {
                    for (int i = 0; i < numFiles; i++) {
                        var offset = reader.ReadInt32();
                        var length = reader.ReadInt32();
                        var pathLength = reader.ReadInt32();
                        var file = Encoding.UTF8.GetString(reader.ReadBytes(pathLength));
                        var path = Path.Combine(target_dir, file);
                        files.Add(file);

                        Directory.CreateDirectory(Path.GetDirectoryName(path));

                        Logger.Debug($"Writing file: '{file}'");
                        using (var fileStream = File.OpenWrite(path)) {
                            if (stream.Position != offset) {
                                stream.Seek(offset, SeekOrigin.Begin);
                            }
                            var decrypted_data = DecryptStream(stream, length, i);
                            fileStream.Write(decrypted_data, 0, decrypted_data.Length);
                        }
                    }
                }
            }

            return files;
        }

         public byte[] DecryptStream(Stream stream, int length, int iv) {
            var buff = new byte[length];
            if (!Decrypt) {
                stream.Read(buff, 0, length);
                return buff;
            }

            var plaintext = new byte[length];
            using (var aesAlg = new Aes128CounterMode(NollaPrng.Get16Seeded(iv))) {
                // Create a decryptor to perform the stream transform.
                var decryptor = aesAlg.CreateDecryptor(AESKey, null);
                var buffer = new byte[length];
                stream.Read(buffer, 0, length);
                using (MemoryStream limitedStream = new MemoryStream(buffer)) {
                    using (CryptoStream csDecrypt = new CryptoStream(limitedStream, decryptor, CryptoStreamMode.Read)) {
                        csDecrypt.Read(plaintext, 0, length);
                    }
                }
            }
            return plaintext;
        }

    }
}
