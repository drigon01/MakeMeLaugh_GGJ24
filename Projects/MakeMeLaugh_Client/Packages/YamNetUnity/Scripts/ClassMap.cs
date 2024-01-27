using UnityEngine;

namespace YamNetUnity
{
    public class ClassMap : ScriptableObject
    {
        [SerializeField]
        private string[] classNames;

        public string this[int classId] => classNames[classId];
        
        public int Length => classNames.Length;
    }
}