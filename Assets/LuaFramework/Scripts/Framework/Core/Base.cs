using UnityEngine;

namespace LuaFramework
{
    public class Base : MonoBehaviour
    {
        private AppFacade m_Facade;

        protected AppFacade facade
        {
            get
            {
                if (m_Facade == null)
                {
                    m_Facade = AppFacade.Instance;
                }
                return m_Facade;
            }
        }
    }
}


