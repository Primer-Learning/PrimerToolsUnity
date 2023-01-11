using UnityEngine;

namespace Primer.Axis
{
    public class AxisArrows
    {
        private static readonly Quaternion originArrowRotation = Quaternion.Euler(0f, -90f, 0f);
        private static readonly Quaternion endArrowRotation = Quaternion.Euler(0f, 90f, 0f);

        private readonly AxisRenderer axis;

        private ArrowPresence? lastPresence;
        private Transform originArrow;
        private Transform endArrow;

        public AxisArrows(AxisRenderer axis) => this.axis = axis;

        public void Update(ChildrenModifier modifier)
        {
            FindOrCreateArrows(modifier, axis.arrowPresence);

            if (endArrow)
                endArrow.transform.localPosition = new Vector3(axis.length + axis.offset, 0f, 0f);

            if (originArrow)
                originArrow.transform.localPosition = new Vector3(axis.offset, 0f, 0f);
        }

        private void FindOrCreateArrows(ChildrenModifier modifier, ArrowPresence presence)
        {
            if (presence == lastPresence) {
                modifier.NextMustBe(originArrow);
                modifier.NextMustBe(endArrow);
                return;
            }

            lastPresence = presence;

            if (presence == ArrowPresence.Neither) {
                endArrow = null;
                originArrow = null;
                return;
            }

            endArrow = FindOrCreateArrow(modifier.Next(endArrow), "End Arrow", endArrowRotation);

            if (presence == ArrowPresence.Positive) {
                originArrow = null;
                return;
            }

            originArrow = FindOrCreateArrow(modifier.Next(originArrow), "Origin Arrow", originArrowRotation);
        }

        private Transform FindOrCreateArrow(IChildDefinition definition, string name, Quaternion rotation)
        {
            return definition
                .Called(name)
                .Initialize(x => x.localRotation = rotation)
                .InstantiatedFrom(axis.arrowPrefab);
        }
    }
}
