using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ThreadedDataRequestor: MonoBehaviour
{
	static ThreadedDataRequestor instance;
	Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

    private void Awake()
    {
		instance = FindObjectOfType<ThreadedDataRequestor>();
    }

    public static void RequestData(Func<object> generateData, Action<object> callback)
	{
		ThreadStart threadStart = delegate {
			instance.DataThread(generateData, callback);
		};

		new Thread(threadStart).Start();
	}

	void DataThread(Func<object> generateData, Action<object> callback)
	{
		object data = generateData();
		lock (dataQueue)
		{
			dataQueue.Enqueue(new ThreadInfo(callback, data));
		}
	}

	struct ThreadInfo
	{
		public readonly Action<object> callback;
		public readonly object parameter;

		public ThreadInfo(Action<object> callback, object parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}
	void Update()
	{
		if (dataQueue.Count > 0)
		{
			for (int i = 0; i < dataQueue.Count; i++)
			{
				ThreadInfo threadInfo = dataQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
	}
}
