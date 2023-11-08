using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Primer.Shapes
{
    public class PrimerArrow3 : MonoBehaviour
    {
        private const float shaftAdjustment = 0.05f;
        
        [SerializeField, HideInInspector]
        private bool _showTailArrow;
        [ShowInInspector]
        public bool showTailArrow
        {
            get => _showTailArrow;
            set
            {
                _showTailArrow = value;
                Update();
            }
        }
        [SerializeField, HideInInspector]
        private bool _showHeadArrow;
        [ShowInInspector]
        public bool showHeadArrow
        {
            get => _showHeadArrow;
            set
            {
                _showHeadArrow = value;
                Update();
            }
        }
        [SerializeField, HideInInspector]
        private float _tailPadding;
        [ShowInInspector]
        public float tailPadding
        {
            get => _tailPadding;
            set
            {
                _tailPadding = value;
                Update();
            }
        }
        
        [SerializeField, HideInInspector]
        private float _headPadding;
        [ShowInInspector]
        public float headPadding
        {
            get => _headPadding;
            set
            {
                _headPadding = value;
                Update();
            }
        }
        
        // private float length;
        [SerializeField, HideInInspector, MinValue(0)]
        private float _width = 1;

        [ShowInInspector]
        public float width
        {
            get => _width;
            set
            {   
                _width = value;
                Update();
            }
        }
        
        // Length and rotation approach
        [ShowInInspector]
        public float length
        {
            get => _tailPoint.magnitude;
            set
            {
                _tailPoint = _tailPoint.normalized * value;
                Update();
            }
        }
        [ShowInInspector]
        public float rotation
        {
            get => transform.localRotation.eulerAngles.z;
            set
            {
                transform.localRotation = Quaternion.Euler(0, 0, value);
                _tailPoint = Quaternion.Euler(0, 0, value) * Vector3.right * length;
                Update();
            }
        }
        
        // Starting point approach
        [FormerlySerializedAs("_startPoint")] [SerializeField, HideInInspector]
        internal Vector3 _tailPoint;
        [ShowInInspector]
        public Vector3 tailPoint
        {
            get => _tailPoint;
            set
            {
                _tailPoint = value;
                // transformThatTailFollows.localPosition = value;
                Update();
            } 
        }
        
        [SerializeField]
        private Transform shaftObject;
        [SerializeField]
        private Transform headObject;
        [SerializeField]
        internal Transform tailObject;

        [SerializeField] internal Transform transformThatTailFollows;
        [SerializeField] internal Transform transformThatHeadFollows;
        
        [Button]
        internal void Update()
        {
            if (transformThatHeadFollows != null) transform.position = transformThatHeadFollows.position;
            if (transformThatTailFollows != null)
            {
                if (transform.parent == null)
                    _tailPoint = transformThatTailFollows.position - transform.position;
                else
                {
                    _tailPoint = transform.parent.InverseTransformPoint(transformThatTailFollows.position) - transform.localPosition;
                }
            }
            
            var rotation = Quaternion.Euler(0, 0, Mathf.Atan2(_tailPoint.y, _tailPoint.x) * 180 / Mathf.PI);
            transform.localRotation = rotation;

            headObject.localPosition = Vector3.right * headPadding;
            headObject.localScale = Vector3.one * _width;

            var lengthToCutFromHead = showHeadArrow ? shaftAdjustment * _width + headPadding : headPadding;
            shaftObject.localPosition = Vector3.right * lengthToCutFromHead;
            
            var totalLength = _tailPoint.magnitude;
            var lengthToCutFromTail = _showTailArrow 
                ? shaftAdjustment * _width + tailPadding 
                : tailPadding;
            shaftObject.localScale = new Vector3(totalLength - lengthToCutFromHead - lengthToCutFromTail, _width, 1);
            
            if (showHeadArrow)
            {
                headObject.gameObject.SetActive(true);
                headObject.localPosition = Vector3.zero;
                headObject.localScale = Vector3.one * _width;
            }
            else
            {
                headObject.gameObject.SetActive(false);
            }
            if (showTailArrow)
            {
                tailObject.gameObject.SetActive(true);
                tailObject.localPosition = Vector3.right * (totalLength - tailPadding);
                tailObject.localScale = Vector3.one * _width;
            }
            else
            {
                tailObject.gameObject.SetActive(false);
            }
        }
    }
}