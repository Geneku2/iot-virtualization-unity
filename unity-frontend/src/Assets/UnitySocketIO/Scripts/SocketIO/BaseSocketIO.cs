using UnityEngine;
using System;
using System.Collections;
using UnitySocketIO.Events;
using System.Collections.Generic;

namespace UnitySocketIO.SocketIO {
    public class BaseSocketIO : MonoBehaviour {

        public string SocketID { get; set; }

        protected SocketIOSettings settings;
        protected Dictionary<string, string> headers;
        
        public virtual void Init(SocketIOSettings settings, Dictionary<string, string> headers = null) {
            this.settings = settings;
            this.headers = headers;
        }

        public virtual void Connect() { }
        public virtual void Close() { }

        public virtual void Emit(string e) { }
        public virtual void Emit(string e, Action<string> action) { }
        public virtual void Emit(string e, string data) { }
        public virtual void Emit(string e, string data, Action<string> action) { }

        public virtual void On(string e, Action<SocketIOEvent> callback) { }
        public virtual void Off(string e, Action<SocketIOEvent> callback) { }
        
    }
}