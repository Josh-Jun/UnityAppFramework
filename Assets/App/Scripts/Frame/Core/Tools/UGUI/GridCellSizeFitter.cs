#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.UI
{
    public class GridCellSizeFitter : MonoBehaviour
    {
        private GridLayoutGroup self;

        private void Awake()
        {
            Init();
        }

        private void Start()
        {

        }

        public void Init()
        {
            self = GetComponent<GridLayoutGroup>();
            if(self == null) return;
            Rect rect = (transform.parent as RectTransform).rect;
            self.cellSize = new Vector2(rect.width, rect.height);
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(GridCellSizeFitter))]
    [InitializeOnLoad]
    public class GridCellSizeFitterEditor : Editor
    {
        private GridCellSizeFitter fitter;

        void OnEnable()
        {
            fitter = (GridCellSizeFitter)(target);
            EditorApplication.update += Update;
        }

        void Update()
        {
            fitter.Init();
        }
    }
#endif
}