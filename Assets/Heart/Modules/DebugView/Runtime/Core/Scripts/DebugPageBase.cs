using System.Collections.Generic;
using Pancake.UI;
using UnityEngine;
using Pancake.DebugView;

namespace Pancake.DebugView
{
    public abstract class DebugPageBase : Page, IRecyclerViewCellProvider, IRecyclerViewDataProvider
    {
        private readonly LinkedList<int> _itemIds = new LinkedList<int>();
        private readonly Dictionary<int, int> _itemIdToDataIndexMap = new Dictionary<int, int>();
        private readonly List<ItemInfo> _itemInfos = new List<ItemInfo>();
        private readonly Dictionary<int, string> _objIdToPrefabKeyMap = new Dictionary<int, string>();

        private readonly Dictionary<string, ObjectPool<GameObject>> _prefabPools = new Dictionary<string, ObjectPool<GameObject>>();

        private bool _addedOrRemovedInThisFrame;

        private int _currentItemId;
        private string _overrideTitle;
        private GameObject _poolRoot;
        private PrefabContainer _prefabContainer;
        private RecyclerView _recyclerView;

        protected abstract string Title { get; }

        public IReadOnlyList<ItemInfo> ItemInfos => _itemInfos;

        protected RecyclerView RecyclerView
        {
            get
            {
                if (_recyclerView == null)
                {
                    _recyclerView = GetComponentInChildren<RecyclerView>();
                    _recyclerView.DataCount = 0;
                    _recyclerView.CellProvider = this;
                    _recyclerView.DataProvider = this;
                }

                return _recyclerView;
            }
        }

        protected virtual void Awake()
        {
            _poolRoot = new GameObject("PoolRoot");
            _poolRoot.transform.SetParent(transform);
            _prefabContainer = GetComponent<PrefabContainer>();
        }

        protected virtual void Start()
        {
            // Add padding for the safe area.
            var canvasScaleFactor = GetComponentInParent<Canvas>().scaleFactor;
            RecyclerView.AfterPadding += (int) (Screen.safeArea.y / canvasScaleFactor);
        }

        protected virtual void LateUpdate()
        {
            if (_addedOrRemovedInThisFrame)
                Reload();

            _addedOrRemovedInThisFrame = false;
        }

        protected virtual void OnDestroy()
        {
            foreach (var pool in _prefabPools.Values)
                pool.Clear();
        }

        GameObject IRecyclerViewCellProvider.GetCell(int dataIndex)
        {
            var prefabKey = _itemInfos[dataIndex].prefabKey;
            if (!_prefabPools.TryGetValue(prefabKey, out var pool))
            {
                pool = new ObjectPool<GameObject>(() =>
                {
                    var prefab = _prefabContainer.GetPrefab(prefabKey);
                    return Instantiate(prefab);
                });
                _prefabPools.Add(prefabKey, pool);
            }

            var obj = pool.Use();
            _objIdToPrefabKeyMap[obj.GetInstanceID()] = prefabKey;
            obj.SetActive(true);
            return obj;
        }

        void IRecyclerViewCellProvider.ReleaseCell(GameObject obj)
        {
            var prefabKey = _objIdToPrefabKeyMap[obj.GetInstanceID()];
            var pool = _prefabPools[prefabKey];
            pool.Release(obj);
            obj.SetActive(false);
            obj.transform.SetParent(_poolRoot.transform);
        }

        void IRecyclerViewDataProvider.SetupCell(int dataIndex, GameObject cell)
        {
            var data = _itemInfos[dataIndex].cellModel;
            cell.GetComponent<ICell>().Setup(data);
        }

        /// <summary>
        ///     Get the instance of the cell at the specified item id.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns>GameObject if exits, null if not.</returns>
        public GameObject GetCellIfExists(int itemId)
        {
            if (!_itemIdToDataIndexMap.TryGetValue(itemId, out var dataIndex))
                return null;

            return RecyclerView.GetCellIfExists(dataIndex);
        }

        /// <summary>
        ///     Add a item.
        /// </summary>
        /// <param name="prefabKey"></param>
        /// <param name="model"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public int AddItem(string prefabKey, CellModel model)
        {
            var itemId = _currentItemId++;
            var node = _itemIds.AddLast(itemId);
            var previousItemId = node.Previous?.Value;
            var index = previousItemId.HasValue ? _itemInfos.FindIndex(x => x.itemId == previousItemId.Value) + 1 : 0;

            _itemInfos.Insert(index, new ItemInfo(itemId, prefabKey, model));
            _itemIdToDataIndexMap[itemId] = index;
            RecyclerView.DataCount++;
            _addedOrRemovedInThisFrame = true;
            return itemId;
        }

        /// <summary>
        ///     Remove a item.
        /// </summary>
        /// <param name="itemId"></param>
        public void RemoveItem(int itemId)
        {
            var info = _itemInfos.Find(x => x.itemId == itemId);
            _itemIds.Remove(info.itemId);
            _itemIdToDataIndexMap.Remove(info.itemId);
            _itemInfos.Remove(info);
            RecyclerView.DataCount--;
            _addedOrRemovedInThisFrame = true;
        }

        /// <summary>
        ///     Remove all items.
        /// </summary>
        public void ClearItems()
        {
            _itemIds.Clear();
            _itemInfos.Clear();
            _itemIdToDataIndexMap.Clear();
            RecyclerView.DataCount = 0;
            _addedOrRemovedInThisFrame = true;
        }

        /// <summary>
        ///     Delete and re-generate cells.
        /// </summary>
        public void Reload() { RecyclerView.Reload(); }

        /// <summary>
        ///     Update the data of the displayed cells.
        /// </summary>
        public void RefreshData() { RecyclerView.RefreshData(); }

        /// <summary>
        ///     Update only a data of specified index.
        /// </summary>
        /// <param name="dataIndex"></param>
        public void RefreshDataAt(int dataIndex) { RecyclerView.RefreshDataAt(dataIndex); }

        public void SetTitle(string title) { _overrideTitle = title; }

        public string GetTitle() { return string.IsNullOrEmpty(_overrideTitle) ? Title : _overrideTitle; }

        public sealed class ItemInfo
        {
            public readonly CellModel cellModel;
            public readonly int itemId;
            public readonly string prefabKey;

            public ItemInfo(int itemId, string prefabKey, CellModel cellModel)
            {
                this.itemId = itemId;
                this.prefabKey = prefabKey;
                this.cellModel = cellModel;
            }
        }
    }
}