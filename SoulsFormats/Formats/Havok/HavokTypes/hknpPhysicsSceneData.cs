using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    public class hknpPhysicsSceneData : hkReferencedObject
    {
        object[] systemDatas;
        object worldCinfo;

        public hknpPhysicsSceneData(HavokTagObject o) : base(o) { }

        public override void Read(byte[] data, BinaryReaderEx r, HavokFile f)
        {
            base.Read(data, r, f);

            ReadPtrArr(out systemDatas, r, f);

            var wciPtr = r.ReadInt64();
            var wciInd = f.ObjectPtrs[(int)wciPtr];
            worldCinfo = f.Objects[wciInd];
            if (wciInd != 0) throw new Exception("wciInd was not null in hknpPhysicsSceneData");
        }
    }
}
