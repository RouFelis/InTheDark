using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace InTheDark.Prototypes
{
	// 수정 예정
	public class EventManager : MonoBehaviour
	{
		private class EventHandler : Singleton<EventHandler>
		{
			private Dictionary<string, EventDelegate> _events = new Dictionary<string, EventDelegate>();

			public void Add(string name, EventDelegate onEvent)
			{
				if (_events.ContainsKey(name))
				{
					_events[name] += onEvent;
				}
				else
				{
					_events.Add(name, onEvent);
				}
			}

			public void Remove(string name, EventDelegate onEvent)
			{
				if (_events.ContainsKey(name))
				{
					_events[name] -= onEvent;
				}
			}

			public void Raise(string name)
			{
				var onEvent = _events[name];

				onEvent?.Invoke();
			}

			public void Clear()
			{
				_events.Clear();
			}
		}

		private class EventHandler<T> : Singleton<EventHandler<T>>
		{
			private Dictionary<string, EventDelegate<T>> _events = new Dictionary<string, EventDelegate<T>>();

			public void Add(string name, EventDelegate<T> onEvent)
			{
				if (_events.ContainsKey(name))
				{
					_events[name] += onEvent;
				}
				else
				{
					_events.Add(name, onEvent);
				}
			}

			public void Remove(string name, EventDelegate<T> onEvent)
			{
				if (_events.ContainsKey(name))
				{
					_events[name] -= onEvent;
				}
			}

			public void Raise(string name, T value)
			{
				var onEvent = _events[name];

				onEvent?.Invoke(value);
			}

			public void Clear()
			{
				_events.Clear();
			}
		}

		private class EventHandler<T, U> : Singleton<EventHandler<T, U>>
		{
			private Dictionary<string, EventDelegate<T, U>> _events = new Dictionary<string, EventDelegate<T, U>>();

			public void Add(string name, EventDelegate<T, U> onEvent)
			{
				if (_events.ContainsKey(name))
				{
					_events[name] += onEvent;
				}
				else
				{
					_events.Add(name, onEvent);
				}
			}

			public void Remove(string name, EventDelegate<T, U> onEvent)
			{
				if (_events.ContainsKey(name))
				{
					_events[name] -= onEvent;
				}
			}

			public void Raise(string name, T value0, U value1)
			{
				var onEvent = _events[name];

				onEvent?.Invoke(value0, value1);
			}

			public void Clear()
			{
				_events.Clear();
			}
		}

		public delegate void EventDelegate();
		public delegate void EventDelegate<T>(T parameter0);
		public delegate void EventDelegate<T, U>(T parameter0, U parameter1);

		private static EventManager _instance;

		public static EventManager Instance
		{
			get
			{
				return _instance;
			}
		}

		static EventManager()
		{
			_instance = default;
		}

		private void Awake()
		{
			OnInitialize();
		}

		private void OnInitialize()
		{
			if (!_instance)
			{
				_instance = this;

				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public void Add(string name, EventDelegate onEvent)
		{
			EventHandler.Instance?.Add(name, onEvent);
		}

		public void Add<T>(string name, EventDelegate<T> onEvent)
		{
			EventHandler<T>.Instance?.Add(name, onEvent);
		}

		public void Add<T, U>(string name, EventDelegate<T, U> onEvent)
		{
			EventHandler<T, U>.Instance?.Add(name, onEvent);
		}

		public void Raise(string name)
		{
			EventHandler.Instance?.Raise(name);
		}

		public void Raise<T>(string name, T value)
		{
			EventHandler<T>.Instance?.Raise(name, value);
		}

		public void Raise<T, U>(string name, T value0, U value1)
		{
			EventHandler<T, U>.Instance?.Raise(name, value0, value1);
		}
	}
}