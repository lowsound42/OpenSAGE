﻿using System;
using System.Collections.Generic;
using OpenSage.Input;
using OpenSage.Mathematics;
using Veldrid;
using Veldrid.Sdl2;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage
{
    public unsafe sealed class Sdl2GameWindow : GameWindow
    {
        private readonly Sdl2Window _window;

        private readonly Queue<GameMessage> _messageQueue = new Queue<GameMessage>();

        private bool _closing;
        private int _lastMouseX;
        private int _lastMouseY;

        public override Rectangle ClientBounds
        {
            get
            {
                var result = _window.Bounds;
                return new Rectangle(0, 0, result.Width, result.Height);
            }
        }

        public override bool IsMouseVisible
        {
            get => Sdl2Native.SDL_ShowCursor(Sdl2Native.SDL_QUERY) == 1;
            set => Sdl2Native.SDL_ShowCursor(value ? Sdl2Native.SDL_ENABLE : Sdl2Native.SDL_DISABLE);
        }

        public override IntPtr NativeWindowHandle => _window.Handle;

        public Sdl2GameWindow(IntPtr windowsWindowHandle)
        {
            _window = new Sdl2Window(windowsWindowHandle, false);
            AfterWindowCreated();
        }

        public Sdl2GameWindow(string title, int x, int y, int width, int height)
        {
            _window = new Sdl2Window(title, x, y, width, height, (SDL_WindowFlags) 0, false);
            AfterWindowCreated();
        }

        private void AfterWindowCreated()
        {
            _window.KeyDown += HandleKeyDown;
            _window.KeyUp += HandleKeyUp;

            _window.MouseDown += HandleMouseDown;
            _window.MouseUp += HandleMouseUp;
            _window.MouseMove += HandleMouseMove;
            _window.MouseWheel += HandleMouseWheel;

            _window.Resized += HandleResized;

            _window.Closing += HandleClosing;
        }

        private void HandleClosing()
        {
            _closing = true;
        }

        private void HandleResized()
        {
            RaiseClientSizeChanged();
        }

        private void HandleKeyDown(KeyEvent evt)
        {
            var message = new GameMessage(GameMessageType.KeyDown);
            message.AddIntegerArgument((int) evt.Key);
            _messageQueue.Enqueue(message);
        }

        private void HandleKeyUp(KeyEvent evt)
        {
            var message = new GameMessage(GameMessageType.KeyUp);
            message.AddIntegerArgument((int) evt.Key);
            _messageQueue.Enqueue(message);
        }

        private void HandleMouseDown(MouseEvent evt)
        {
            GameMessageType? getMessageType()
            {
                switch (evt.MouseButton)
                {
                    case MouseButton.Left:
                        return GameMessageType.MouseLeftButtonDown;
                    case MouseButton.Middle:
                        return GameMessageType.MouseMiddleButtonDown;
                    case MouseButton.Right:
                        return GameMessageType.MouseRightButtonDown;
                    default:
                        return null;
                }
            }

            var messageType = getMessageType();
            if (messageType == null)
            {
                return;
            }

            var message = new GameMessage(messageType.Value);
            message.AddScreenPositionArgument(new Point2D(_lastMouseX, _lastMouseY));
            _messageQueue.Enqueue(message);
        }

        private void HandleMouseUp(MouseEvent evt)
        {
            GameMessageType? getMessageType()
            {
                switch (evt.MouseButton)
                {
                    case MouseButton.Left:
                        return GameMessageType.MouseLeftButtonUp;
                    case MouseButton.Middle:
                        return GameMessageType.MouseMiddleButtonUp;
                    case MouseButton.Right:
                        return GameMessageType.MouseRightButtonUp;
                    default:
                        return null;
                }
            }

            var messageType = getMessageType();
            if (messageType == null)
            {
                return;
            }

            var message = new GameMessage(messageType.Value);
            message.AddScreenPositionArgument(new Point2D(_lastMouseX, _lastMouseY));
            _messageQueue.Enqueue(message);
        }

        private void HandleMouseMove(MouseMoveEventArgs args)
        {
            var message = new GameMessage(GameMessageType.MouseMove);
            message.AddScreenPositionArgument(new Point2D(args.State.X, args.State.Y));
            _messageQueue.Enqueue(message);
        }

        private void HandleMouseWheel(MouseWheelEventArgs args)
        {
            var message = new GameMessage(GameMessageType.MouseWheel);
            message.AddIntegerArgument((int) (args.WheelDelta * 100));
            _messageQueue.Enqueue(message);
        }

        public override void SetCursor(Cursor cursor)
        {
            // TODO
        }

        public override bool PumpEvents()
        {
            // TODO: Use inputSnapshot instead of events?
            var inputSnapshot = _window.PumpEvents();

            // TODO: This isn't right, it means button events might not have the right position.
            _lastMouseX = (int) inputSnapshot.MousePosition.X;
            _lastMouseY = (int) inputSnapshot.MousePosition.Y;

            if (_closing)
            {
                return false;
            }

            while (_messageQueue.Count > 0)
            {
                RaiseInputMessageReceived(new InputMessageEventArgs(_messageQueue.Dequeue()));
            }

            return true;
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            // TODO: This isn't right.
            _window.Close();

            base.Dispose(disposeManagedResources);
        }
    }
}
