using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class AttachmentSlot {



    public Attachment occupant;

    public int diameter;

    public Core topLeft;
    public Core topRight;
    public Core bottomLeft;
    public Core bottomRight;

    public AttachmentSlot(int diameterVal, Core topLeftCore, Core topRightCore, Core bottomLeftCore, Core bottomRightCore)
    {
        this.diameter = diameterVal;
        this.topLeft = topLeftCore;
        this.topRight = topRightCore;
        this.bottomLeft = bottomLeftCore;
        this.bottomRight = bottomRightCore;
    }

    public bool ContainsCore(Core core)
    {
        return topLeft == core || topRight == core || bottomLeft == core || bottomRight == core;
    }

    public bool IsThisSlot(Core one, Core two, Core three, Core four)
    {
        int count = 0;
        count += topLeft == one || topLeft == two || topLeft == three || topLeft == four ? 1 : 0;
        count += topRight == one || topRight == two || topRight == three || topRight == four ? 1 : 0;
        count += bottomLeft == one || bottomLeft == two || bottomLeft == three || bottomLeft == four ? 1 : 0;
        count += bottomRight == one || bottomRight == two || bottomRight == three || bottomRight == four ? 1 : 0;

        return count == 4;
    }


}*/
