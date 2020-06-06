using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV2Obj
{
    class Program
    {
        static void Main(string[] args)
        {
            string cfgPath = "config.txt";
            if (!File.Exists(cfgPath))
                cfgPath = Path.Combine(new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName, "config.txt");

            if(!File.Exists(cfgPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Config file not exists!");

                Console.ReadLine();
                return;
            }

            string[] config = File.ReadAllLines("config.txt");

            if(config.Length != 7 && config.Length != 10)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Config file not correct!");

                Console.ReadLine();
                return;
            }

            foreach(var arg in args)
            {
                var fileInfo = new FileInfo(arg);

                if(fileInfo.Extension.ToLower() == ".csv" && fileInfo.Exists)
                {
                    DoConvert(fileInfo, config);
                }
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        public static string Repeat(char ch, int count)
        {
            string str = "";

            for (int i = 0; i < count; i++)
                str += ch;

            return str;
        }

        private static void DoConvert(FileInfo fileInfo, string[] config)
        {
            Console.WriteLine($"Converting {fileInfo.Name}...");

            float scale = float.Parse(config[6]);

            bool firstLine = true;

            var keyValues = new Dictionary<string, int>();

            var vertexes = new List<Vertex>();
            var uvs = new List<UV>();
            var normals = new List<Normal>();

            var faces = new List<Face>();

            var hasNormal = config.Length == 10;

            // Read all vertex and uvs
            using (var sr = fileInfo.OpenText())
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    var cols = line.Split(',');

                    if (firstLine)
                    {
                        firstLine = false;

                        for (int i = 0; i < cols.Length; i++)
                        {
                            keyValues.Add(cols[i].Trim(), i);
                        }
                    }
                    else if (cols.Length == keyValues.Count)
                    {
                        int idx = int.Parse(cols[keyValues[config[5]]].Trim());

                        var v = new Vertex
                        {
                            index = idx + 1,

                            pos = new float3
                            {
                                x = float.Parse(cols[keyValues[config[0]]].Trim()) * scale,
                                y = float.Parse(cols[keyValues[config[1]]].Trim()) * scale,
                                z = float.Parse(cols[keyValues[config[2]]].Trim()) * scale,
                            }
                        };

                        var t = new UV
                        {
                            index = idx + 1,

                            uv = new float3
                            {
                                x = float.Parse(cols[keyValues[config[3]]].Trim()),
                                y = 1.0f - float.Parse(cols[keyValues[config[4]]].Trim())
                            }
                        };

                        vertexes.Add(v);
                        uvs.Add(t);

                        if(hasNormal)
                        {
                            var n = new Normal
                            {
                                index = idx + 1,

                                dir = new float3
                                {
                                    x = float.Parse(cols[keyValues[config[7]]].Trim()),
                                    y = float.Parse(cols[keyValues[config[8]]].Trim()),
                                    z = float.Parse(cols[keyValues[config[9]]].Trim()),
                                }
                            };

                            normals.Add(n);
                        }
                    }
                }
            }

            // Group vertex and uvs in to a triangle face and calculate normal
            for(int i = 0; i < vertexes.Count; i += 3)
            {
                // IDK why
                if (vertexes[i].pos == vertexes[i + 1].pos)
                    continue;

                // Build face
                var f = new Face
                {
                    vc = 3,
                    v = new Vertex[] {
                        vertexes[i],
                        vertexes[i + 1],
                        vertexes[i + 2]
                    },
                    t = new UV[] {
                        uvs[i],
                        uvs[i + 1],
                        uvs[i + 2]
                    }
                };

                if(hasNormal)
                {
                    f.n = new Normal[] {
                        normals[i],
                        normals[i + 1],
                        normals[i + 2],
                    };
                }

                faces.Add(f);
            }

            // Center the model
            float3 sum = new float3();
            long count = 0;

            foreach (var f in faces)
            {
                foreach (var v in f.v)
                {
                    sum += v.pos;

                    count++;
                }
            }

            var offset = sum / count;

            foreach (var v in vertexes)
            {
                v.pos -= offset;
            }

            // Build the obj
            var objBuilder = new StringBuilder();

            objBuilder.AppendLine("# Converted by CSV2OBJ By GEEKiDoS");
            objBuilder.AppendLine($"# Convertor config:");

            int maxLen = 0;
            foreach (var name in config)
                if (name.Length > maxLen)
                    maxLen = name.Length;

            maxLen += 4;

            objBuilder.Append($"# IDX{Repeat(' ', maxLen - 3)}");
            objBuilder.Append($"X{Repeat(' ', maxLen - 1)}");
            objBuilder.Append($"Y{Repeat(' ', maxLen - 1)}");
            objBuilder.Append($"Z{Repeat(' ', maxLen - 1)}");
            objBuilder.Append($"U{Repeat(' ', maxLen - 1)}");
            objBuilder.Append($"V{Repeat(' ', maxLen - 1)}\n# ");
            objBuilder.Append($"{config[5]}{Repeat(' ', maxLen - config[5].Length)}");
            objBuilder.Append($"{config[0]}{Repeat(' ', maxLen - config[0].Length)}");
            objBuilder.Append($"{config[1]}{Repeat(' ', maxLen - config[1].Length)}");
            objBuilder.Append($"{config[2]}{Repeat(' ', maxLen - config[2].Length)}");
            objBuilder.Append($"{config[3]}{Repeat(' ', maxLen - config[3].Length)}");
            objBuilder.Append($"{config[4]}{Repeat(' ', maxLen - config[4].Length)}\n\n");

            // Write vertexs
            foreach (var v in vertexes)
                objBuilder.AppendLine(v.ToString());

            objBuilder.AppendLine($"# Vertex count:{ vertexes.Count }\n");
          
            // Write normals
            if(hasNormal)
            {
                foreach (var n in normals)
                    objBuilder.AppendLine(n.ToString());

                objBuilder.AppendLine($"# Calculated normal count:{ normals.Count }\n");
            }

            // Write uvs
            foreach (var t in uvs)
                objBuilder.AppendLine(t.ToString());

            objBuilder.AppendLine($"# UV count:{ uvs.Count }\n");

            // Write group infomation
            objBuilder.AppendLine("g mesh");
            objBuilder.AppendLine("usemtl mesh");
            objBuilder.AppendLine("s off");

            // Write faces
            foreach (var f in faces)
                objBuilder.AppendLine(f.ToString());

            objBuilder.Append($"# Face count:{ faces.Count }");

            // Save
            File.WriteAllText(fileInfo.FullName.Replace(fileInfo.Extension, ".obj"), objBuilder.ToString());
        }
    }
}
