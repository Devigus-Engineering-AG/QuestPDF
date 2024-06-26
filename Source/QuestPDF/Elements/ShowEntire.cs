﻿using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace QuestPDF.Elements
{
    internal sealed class ShowEntire : ContainerElement
    {
        internal override SpacePlan Measure(Size availableSpace)
        {
            var childMeasurement = base.Measure(availableSpace);

            if (childMeasurement.Type is SpacePlanType.Empty or SpacePlanType.FullRender)
                return childMeasurement;

            return SpacePlan.Wrap();
        }
    }
}