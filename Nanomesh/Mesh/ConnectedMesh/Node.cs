﻿namespace Nanomesh
{
    public partial class ConnectedMesh
    {
        public struct Node
        {
            public int position;
            public int sibling;
            public int relative;
            public int attribute;

            public void MarkRemoved()
            {
                position = -10;
            }

            public bool IsRemoved => position == -10;

            public override string ToString()
            {
                return $"sibl:{sibling} rela:{relative} posi:{position}";
            }
        }
    }
}