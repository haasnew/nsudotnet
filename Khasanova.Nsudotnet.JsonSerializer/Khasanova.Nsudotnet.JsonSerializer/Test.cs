using System;

namespace JsonSerializer
{
    [Serializable]
    public class Test
    {
        public int I = 1000;
        public string Str = "Assel!";
        public int[] EmptyArray;
        [NonSerialized]
        public string Ignore; // это поле не должно сериализоваться

        public int[] ArrayMember = { 1, 2, 3 };
        public Serializer SerializerTest;
    }
}


