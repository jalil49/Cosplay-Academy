using MessagePack;
using System;

namespace Cosplay_Academy.Support
{
    public static class Pushup
    {
        [Serializable]
        [MessagePackObject]
        public class BodyData
        {
            [Key("Size")]
            public float Size { get; set; }
            [Key("VerticalPosition")]
            public float VerticalPosition { get; set; }
            [Key("HorizontalAngle")]
            public float HorizontalAngle { get; set; }
            [Key("HorizontalPosition")]
            public float HorizontalPosition { get; set; }
            [Key("VerticalAngle")]
            public float VerticalAngle { get; set; }
            [Key("Depth")]
            public float Depth { get; set; }
            [Key("Roundness")]
            public float Roundness { get; set; }

            [Key("Softness")]
            public float Softness { get; set; }
            [Key("Weight")]
            public float Weight { get; set; }

            [Key("AreolaDepth")]
            public float AreolaDepth { get; set; }
            [Key("NippleWidth")]
            public float NippleWidth { get; set; }
            [Key("NippleDepth")]
            public float NippleDepth { get; set; }

            public BodyData() { }
            public BodyData(ChaFileBody baseBody)
            {
                Softness = baseBody.bustSoftness;
                Weight = baseBody.bustWeight;
                Size = baseBody.shapeValueBody[PushupConstants.IndexSize];
                VerticalPosition = baseBody.shapeValueBody[PushupConstants.IndexVerticalPosition];
                HorizontalAngle = baseBody.shapeValueBody[PushupConstants.IndexHorizontalAngle];
                HorizontalPosition = baseBody.shapeValueBody[PushupConstants.IndexHorizontalPosition];
                VerticalAngle = baseBody.shapeValueBody[PushupConstants.IndexVerticalAngle];
                Depth = baseBody.shapeValueBody[PushupConstants.IndexDepth];
                Roundness = baseBody.shapeValueBody[PushupConstants.IndexRoundness];
                AreolaDepth = baseBody.shapeValueBody[PushupConstants.IndexAreolaDepth];
                NippleWidth = baseBody.shapeValueBody[PushupConstants.IndexNippleWidth];
                NippleDepth = baseBody.shapeValueBody[PushupConstants.IndexNippleDepth];
            }
        }

        [Serializable]
        [MessagePackObject]
        public class ClothData : BodyData
        {
            [Key("Firmness")]
            public float Firmness { get; set; }
            [Key("Lift")]
            public float Lift { get; set; }
            [Key("PushTogether")]
            public float PushTogether { get; set; }
            [Key("Squeeze")]
            public float Squeeze { get; set; }
            [Key("CenterNipples")]
            public float CenterNipples { get; set; }

            [Key("EnablePushup")]
            public bool EnablePushup { get; set; }
            [Key("FlattenNipples")]
            public bool FlattenNipples { get; set; }

            [Key("UseAdvanced")]
            public bool UseAdvanced { get; set; }

            public ClothData() { }
            public ClothData(ChaFileBody baseBody) : base(baseBody) { }
            public ClothData(BodyData bodyData)
            {
                Size = bodyData.Size;
                VerticalPosition = bodyData.VerticalPosition;
                HorizontalAngle = bodyData.HorizontalAngle;
                HorizontalPosition = bodyData.HorizontalPosition;
                VerticalAngle = bodyData.VerticalAngle;
                Depth = bodyData.Depth;
                Roundness = bodyData.Roundness;
                Softness = bodyData.Softness;
                Weight = bodyData.Weight;
                AreolaDepth = bodyData.AreolaDepth;
                NippleWidth = bodyData.NippleWidth;
                NippleDepth = bodyData.NippleDepth;
            }

            public void CopyTo(ClothData data)
            {
                data.Firmness = Firmness;
                data.Lift = Lift;
                data.PushTogether = PushTogether;
                data.Squeeze = Squeeze;
                data.CenterNipples = CenterNipples;
                data.EnablePushup = EnablePushup;
                data.FlattenNipples = FlattenNipples;
                data.UseAdvanced = UseAdvanced;
            }
        }

        internal class PushupConstants
        {
            internal const int IndexSize = 4;
            internal const int IndexVerticalPosition = 5;
            internal const int IndexHorizontalAngle = 6;
            internal const int IndexHorizontalPosition = 7;
            internal const int IndexVerticalAngle = 8;
            internal const int IndexDepth = 9;
            internal const int IndexRoundness = 10;
            internal const int IndexAreolaDepth = 11;
            internal const int IndexNippleWidth = 12;
            internal const int IndexNippleDepth = 13;

            //Strings used in ExtensibleSaveFormat data
            internal const string Pushup_BraData = "Pushup_BraData";
            internal const string Pushup_TopData = "Pushup_TopData";
            internal const string Pushup_BodyData = "Pushup_BodyData";
            internal const string PushupCoordinate_BraData = "PushupCoordinate_BraData";
            internal const string PushupCoordinate_TopData = "PushupCoordinate_TopData";
        }
    }
}
