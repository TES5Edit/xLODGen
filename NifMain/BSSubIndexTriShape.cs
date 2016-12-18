using System;
using System.IO;
using System.Collections.Generic;

namespace LODGenerator.NifMain
{
    public class BSSubIndexTriShape : BSTriShape
    {
        protected uint numTriangles2;
        protected uint numA;
        protected uint numB;
        protected List<BSSITSSegment> segments;

        public BSSubIndexTriShape()
        {
            this.numTriangles2 = 0;
            this.numA = 0;
            this.numB = 0;
            this.segments = new List<BSSITSSegment>();
        }

        public override void Read(NiHeader header, BinaryReader reader)
        {
            base.Read(header, reader);
            this.numTriangles2 = reader.ReadUInt32();
            this.numA = reader.ReadUInt32();
            this.numB = reader.ReadUInt32();
            for (int index = 0; index < this.numA; ++index)
            {
                this.segments.Add(new BSSITSSegment(0, 0)
                {
                    triangleOffset = reader.ReadUInt32(),
                    triangleCount = reader.ReadUInt32(),
                    unknownHash = reader.ReadUInt32(),
                    numSegments = reader.ReadUInt32(),
                });
                //subSegment = reader.ReadInt32(),
            }
        }

        public override void Write(NiHeader header, BinaryWriter writer)
        {
            base.Write(header, writer);
            writer.Write(this.numTriangles2);
            writer.Write(this.numA);
            writer.Write(this.numB);
            for (int index = 0; index < this.numA; ++index)
            {
                writer.Write(this.segments[index].triangleOffset);
                writer.Write(this.segments[index].triangleCount);
                writer.Write(this.segments[index].unknownHash);
                writer.Write(this.segments[index].numSegments);
            }
            //writer.Write(this.segments[index].subSegment);
        }

        public override uint GetSize(NiHeader header)
        {
            return base.GetSize(header) + 12 + this.numA * 16;
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

        public void AddSegment(BSSITSSegment segment)
        {
            this.segments.Add(segment);
            ++this.numA;
            ++this.numB;
        }

        public BSSITSSegment GetSegmentAtIndex(int index)
        {
            return this.segments[index];
        }

        public void SetSegment(int index, BSSITSSegment segment)
        {
            this.segments[index] = segment;
        }

        public void RemoveSegment(int index)
        {
            this.segments.RemoveAt(index);
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
