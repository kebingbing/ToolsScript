using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using System;


namespace Phoenix
{

	//计时器
	public abstract class Timer
	{

		#region 属性

		/// <summary>
		/// 索引
		/// </summary>
		private int m_Index;
		public int Index
		{
			get { return m_Index; }
		}

		/// <summary>
		/// 时间间隔
		/// </summary>
		private float m_TimeSpace;
		public float TimeSpace
		{
			get { return m_TimeSpace; }
			set { m_TimeSpace = value; }
		}

		/// <summary>
		/// 消耗的时间
		/// </summary>
		private float m_ElapseTime;

		/// <summary>
		/// 参数
		/// </summary>
		private object[] m_Args;
		public object[] Args
		{
			get { return m_Args; }
			set { m_Args = value; }
		}

		/// <summary>
		/// 执行一次
		/// </summary>
		private bool m_OnlyOneTimes;
		public bool OnlyOneTimes
		{
			get { return m_OnlyOneTimes; }
			set { m_OnlyOneTimes = value; }
		}

		#endregion

		public Timer(int Index, float TimeSpace, bool OnlyOneTimes, params object[] Args)
		{
			m_Index = Index;
			m_TimeSpace = TimeSpace;
			m_OnlyOneTimes = OnlyOneTimes;
			m_Args = Args;
		}

		public void Update(float Elapse)
		{
			m_ElapseTime += Elapse;

			if (m_ElapseTime >= m_TimeSpace)
			{
				OnTimer();

				if (OnlyOneTimes)
				{
					TimerManager.Instance.DestroyTimer(Index);
				}
				else
				{
					m_ElapseTime -= m_TimeSpace;
				}
			}
		}

	    public void Reset(float timeSpace, float elapseTime)
	    {
	        m_TimeSpace = timeSpace;
	        m_ElapseTime = elapseTime;
	    }

		/// <summary>
		/// 定时回调
		/// </summary>
		public abstract void OnTimer();

	};

	public class CSharpTimer : Timer
	{
		private TimerManager.FunctionCallback m_FuncCallback;
		public TimerManager.FunctionCallback FuncCallback
		{
			get { return m_FuncCallback; }
			set { m_FuncCallback = value; }
		}

		public CSharpTimer(int Index, float TimeSpace, bool OnlyOneTimes, TimerManager.FunctionCallback FuncCallback, params object[] Args)
			: base(Index, TimeSpace, OnlyOneTimes, Args)
		{
			m_FuncCallback = FuncCallback;
		}

		/// <summary>
		/// 定时回调
		/// </summary>
		public override void OnTimer()
		{
			if (FuncCallback != null)
			{
				FuncCallback(Args);
			}
		}
	}

	public class LuaTimer : Timer
	{

		/// <summary>
		/// 处理者
		/// </summary>
		private LuaFunction m_Hander;
		public LuaFunction Handler
		{
			get { return m_Hander; }
			set { m_Hander = value; }
		}

		public LuaTimer(int Index, float TimeSpace, bool OnlyOneTimes, LuaFunction Handler, params object[] Args)
			: base(Index, TimeSpace, OnlyOneTimes, Args)
		{
			m_Hander = Handler;
		}

		/// <summary>
		/// 定时回调
		/// </summary>
		public override void OnTimer()
		{
			if (m_Hander != null && m_Hander.IsAlive())
			{
				m_Hander.Call(Args);
			}
		}
	}

	//========================================================================
	//========================================================================

	//计时器系统
	public class TimerManager
	{
		public delegate void FunctionCallback(params object[] Args);
	    public const int InvalidTimerId = -1;

		int m_Index = 0;				//计时器编号
		Dictionary<int, Timer> m_TimerMap = new Dictionary<int, Timer>();	//计时器队列

		List<int> m_InvalidTimerList = new List<int>();       //要删除的计时器队列
		List<Timer> m_AddedTimerList = new List<Timer>();       //添加的计时器队列

		/// <summary>
		/// 获取单例
		/// </summary>
		private static TimerManager m_Instance;
		public static TimerManager Instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = new TimerManager();
				}
				return m_Instance;
			}
		}

		/// <summary>
		/// 更新（单位秒）
		/// </summary>
		public void Update(float Elapse)
		{
			Timer tempTime = null;
            for (int i = 0; i<m_AddedTimerList.Count; i++)
            {
                tempTime = m_AddedTimerList[i];
                m_TimerMap[tempTime.Index] = tempTime;
            }

			m_AddedTimerList.Clear();

            for(int i = 0; i < m_InvalidTimerList.Count; i++ )
            {
                destroyTimer(m_InvalidTimerList[i]);
            }

			m_InvalidTimerList.Clear();

            var itor = m_TimerMap.GetEnumerator();
            try
            {
                while (itor.MoveNext())
                {
                    itor.Current.Value.Update(Elapse);
                }
            }
            finally
            {
                itor.Dispose();
            }
        }

		/// <summary>
		/// 销毁
		/// </summary>
		public void Destroy()
		{
			m_AddedTimerList.Clear();
			m_TimerMap.Clear();
			m_InvalidTimerList.Clear();
		}

		/// <summary>
		/// 重置
		/// </summary>
		public void Reset()
		{
			Destroy();
		}

		//========================================================================
		//========================================================================

		/// <summary>
		/// 创建计时器
		/// </summary>
		public int CreateTimer(float TimeSpace, LuaFunction Handler, params object[] Args)
		{
			Timer timer = new LuaTimer(allocTimerIndex(), TimeSpace, true, Handler, Args);
			m_AddedTimerList.Add(timer);

			return timer.Index;
		}

		/// <summary>
		/// 创建计时器
		/// </summary>
		public int CreateTimer(float TimeSpace, bool OnlyOneTimes, LuaFunction Handler, params object[] Args)
		{
			Timer timer = new LuaTimer(allocTimerIndex(), TimeSpace, OnlyOneTimes, Handler, Args);
			m_AddedTimerList.Add(timer);

			return timer.Index;
		}

		/// <summary>
		/// 创建计时器
		/// </summary>
		public int CreateTimer(float TimeSpace, TimerManager.FunctionCallback Handler, params object[] Args)
		{
			Timer timer = new CSharpTimer(allocTimerIndex(), TimeSpace, true, Handler, Args);
			m_AddedTimerList.Add(timer);

			return timer.Index;
		}

		/// <summary>
		/// 创建计时器
		/// </summary>
		public int CreateTimer(float TimeSpace, bool OnlyOneTimes, TimerManager.FunctionCallback Handler, params object[] Args)
		{
			Timer timer = new CSharpTimer(allocTimerIndex(), TimeSpace, OnlyOneTimes, Handler, Args);
			m_AddedTimerList.Add(timer);

			return timer.Index;
		}

        /// <summary>
        /// 获取timer
        /// </summary>
	    public Timer GetTimer(int timerIndex)
        {
            Timer timer;
            if (m_TimerMap.TryGetValue(timerIndex, out timer))
            {
                return timer;
            }

            timer = m_AddedTimerList.Find(t => t.Index == timerIndex);
            return timer;
        }

		/// <summary>
		/// 销毁计时器
		/// </summary>
		public void DestroyTimer(int Index)
		{
			m_InvalidTimerList.Add(Index);
		}

        /// <summary>
        /// 分配计时器编号
        /// </summary>
        private int allocTimerIndex()
		{
			return ++m_Index;
		}

		/// <summary>
		/// 销毁计时器
		/// </summary>
		private void destroyTimer(int Index)
		{
			m_TimerMap.Remove(Index);
		}

	}

}
