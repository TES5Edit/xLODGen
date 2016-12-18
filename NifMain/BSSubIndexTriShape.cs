using System;
using System.IO;
using System.Collections.Generic;

namespace LODGenerator.NifMain
{
    public class BSSubIndexTriShape : BSTriShape
    {
        protected uint numSegments;
        protected List<BSSegment> BSSegments;
        protected uint numTriangles2;
        protected uint numA;
        protected uint numB;
        protected List<BSSITSSegment> BSSITSSsegments;

        public BSSubIndexTriShape()
        {
            this.numSegments = 0;
            this.BSSegments = new List<BSSegment>();
            this.numTriangles2 = 0;
            this.numA = 0;
            this.numB = 0;
            this.BSSITSSsegments = new List<BSSITSSegment>();
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            if (header.GetUserVersion2() == 100)
            {
                this.numSegments = reader.ReadUInt32();
                for (int index = 0; index < this.numSegments; ++index)
                {
                    this.BSSegments.Add(new BSSegment(0, 0)
                    {
                        unknownByte1 = reader.ReadByte(),
                        startTriangle = reader.ReadUInt32(),
                        numTriangles = reader.ReadUInt32()
                    });
                }
            }
            else
            {
                this.numTriangles2 = reader.ReadUInt32();
                this.numA = reader.ReadUInt32();
                this.numB = reader.ReadUInt32();
                for (int index = 0; index < this.numA; ++index)
                {
                    this.BSSITSSsegments.Add(new BSSITSSegment(0, 0)
                    {
                        triangleOffset = reader.ReadUInt32(),
                        triangleCount = reader.ReadUInt32(),
                        unknownHash = reader.ReadUInt32(),
                        numSegments = reader.ReadUInt32(),
                    });
                    //subSegment = reader.ReadInt32(),
                }
            }
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            if (header.GetUserVersion2() == 100)
            {
                writer.Write(this.numSegments);
                for (int index = 0; index < this.numSegments; ++index)
                {
                    writer.Write(this.BSSegments[index].unknownByte1);
                    writer.Write(this.BSSegments[index].startTriangle);
                    writer.Write(this.BSSegments[index].numTriangles);
                }
            }
            else
            {
                writer.Write(this.numTriangles2);
                writer.Write(this.numA);
                writer.Write(this.numB);
                for (int index = 0; index < this.numA; ++index)
                {
                    writer.Write(this.BSSITSSsegments[index].triangleOffset);
                    writer.Write(this.BSSITSSsegments[index].triangleCount);
                    writer.Write(this.BSSITSSsegments[index].unknownHash);
                    writer.Write(this.BSSITSSsegments[index].numSegments);
                }
                //writer.Write(this.segments[index].subSegment);
            }
        }

        public override uint GetSize(NiHeader header)
        {
            if (header.GetUserVersion2() == 100)
            {
                return base.GetSize(header) + 4 + this.numSegments * 9;
            }
            else
            {
                return base.GetSize(header) + 12 + this.numA * 16;
            }
        }

        public void SetNumSegments(uint value)
        {
            this.numSegments = value;
        }

        public uint GetNumSegtments()
        {
            return this.numSegments;
        }

        public void SetNumTriangles2(uint value)
        {
            this.numTriangles2 = value;
        }

        public uint GetNumA()
        {
            return this.numA;
        }

        public uint GetNumB()
        {
            return this.numB;
        }

        public void AddSegment(BSSegment segment)
        {
            this.BSSegments.Add(segment);
            ++this.numSegments;
        }

        public void AddSegment(BSSITSSegment segment)
        {
            this.BSSITSSsegments.Add(segment);
            ++this.numA;
            ++this.numB;
        }

        public BSSegment GetBSSegmentAtIndex(int index)
        {
            return this.BSSegments[index];
        }

        public BSSITSSegment GetBSSITSSegmentAtIndex(int index)
        {
            return this.BSSITSSsegments[index];
        }

        public void SetSegment(int index, BSSegment segment)
        {
            this.BSSegments[index] = segment;
        }

        public void SetSegment(int index, BSSITSSegment segment)
        {
            this.BSSITSSsegments[index] = segment;
        }

        public void RemoveBSSegment(int index)
        {
            this.BSSegments.RemoveAt(index);
            --this.numSegments;
        }

        public void RemoveBSSITSSegment(int index)
        {
            this.BSSITSSsegments.RemoveAt(index);
            --this.numA;
            --this.numB;
        }

        public override string GetClassName()
        {
            return "BSSubIndexTriShape";
        }

        public override bool IsDerivedType(string type)
        {
            bool flag = base.IsDerivedType(type);
            if (!flag)
            {
                flag = type == "BSSubIndexTriShape";
            }
            return flag;
        }
    }
}
