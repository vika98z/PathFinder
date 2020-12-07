using UnityEngine;
using Priority_Queue;

namespace BaseAI
{
    /// <summary>
    /// Точка пути - изменяем по сравенению с предыдущим проектом
    /// </summary>
    public class PathNode : FastPriorityQueueNode
    {
        public Vector3 Position { get; set; } //  Позиция в глобальных координатах
        public Vector3 Direction { get; set; } //  Направление
        public float TimeMoment { get; set; } //  Момент времени        

        /// <summary>
        /// Родительская вершина - предшествующая текущей в пути от начальной к целевой
        /// </summary>
        public PathNode Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                // if (_parent != null)
                //     distance = _parent.Dist + Vector3.Distance(Position, _parent.Position);
                // else
                //     distance = float.PositiveInfinity;
            }
        } //  Родительский узел

        private float distance = float.PositiveInfinity;
        private PathNode _parent = null;

        public float Dist 
        { 
            get => distance;
            set => distance = value; 
        }

        public bool IsChecked { get; set; }
        public bool IsWalkable { get; set; }
        public float G { get; }  //  Пройденный путь от цели
        public float H { get; }  //  Пройденный путь от цели

        /// <summary>
        /// Конструирование вершины на основе родительской (если она указана)
        /// </summary>
        /// <param name="ParentNode">Если существует родительская вершина, то её указываем</param>
        public PathNode(PathNode ParentNode = null) => Parent = ParentNode;

        /// <summary>
        /// Конструирование вершины на основе родительской (если она указана)
        /// </summary>
        /// <param name="ParentNode">Если существует родительская вершина, то её указываем</param>
        public PathNode(Vector3 currentPosition)
        {
            Position = currentPosition;      //  Позицию задаём
            Direction = Vector3.zero;        //  Направление отсутствует
            TimeMoment = -1.0f;              //  Время отрицательное
            Parent = null;                   //  Родителя нет
            G = 0;
            H = 0;
            IsChecked = false;
        }

        /// <summary>
        /// Расстояние между точками без учёта времени. Со временем - отдельная история
        /// Если мы рассматриваем расстояние до целевой вершины, то непонятно как учитывать время
        /// </summary>
        /// <param name="other">Точка, до которой высчитываем расстояние</param>
        /// <returns></returns>
        public float Distance(PathNode other) => Vector3.Distance(Position, other.Position);
        public float FlooredDistance(PathNode other) => Vector3.Distance(Floor(Position), Floor(other.Position));

        private Vector3 Floor(Vector3 transformPosition)
        {
            return new Vector3(Mathf.Round(transformPosition.x), Mathf.Round(transformPosition.y), Mathf.Round(transformPosition.z));
        }
        /// <summary>
        /// Расстояние между точками без учёта времени. Со временем - отдельная история
        /// Если мы рассматриваем расстояние до целевой вершины, то непонятно как учитывать время
        /// </summary>
        /// <param name="other">Точка, до которой высчитываем расстояние</param>
        /// <returns></returns>
        public float Distance(Vector3 other) => Vector3.Distance(Position, other);

        public bool IsAboveTheGround()
        {
            RaycastHit hit;
            if (Physics.Raycast(Position, new Vector3(0, -1, 0), out hit, 5))
            {
                return true;
            }

            return false;
        }
    }
}