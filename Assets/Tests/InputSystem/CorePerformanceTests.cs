using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Users;

////TODO: add test for domain reload logic

// IMPORTANT: When running in editor, make sure to turn off debugging (disable "Editor Attaching" in
//            editor preferences and restart editor) when running tests here. If debugging is enabled,
//            the code will run A LOT slower.

internal class CorePerformanceTests : InputTestFixture
{
    ////TODO: same test but with several actions listening on each gamepad
    [Test, Performance]
    [Category("Performance")]
    public void Performance_Update10Gamepads()
    {
        const int kNumGamepads = 10;

        var gamepads = new Gamepad[kNumGamepads];
        for (var i = 0; i < kNumGamepads; ++i)
            gamepads[i] = InputSystem.AddDevice<Gamepad>();

        Measure.Method(() =>
        {
            for (var i = 0; i < kNumGamepads; ++i)
                InputSystem.QueueStateEvent(gamepads[i], default(GamepadState));
            InputSystem.Update();
        })
            .MeasurementCount(100)
            .Run();
    }

    [Test, Performance]
    [Category("Performance")]
    public void Performance_UpdateMouse100TimesInFrame()
    {
        var mouse = InputSystem.AddDevice<Mouse>();

        Measure.Method(() =>
        {
            for (var i = 0; i < 100; ++i)
                InputSystem.QueueStateEvent(mouse, default(MouseState));
            InputSystem.Update();
        })
            .MeasurementCount(100)
            .Run();
    }

    [Test, Performance]
    [Category("Performance")]
    [TestCase(true)]
    [TestCase(false)]
    public void Performance_TwoTouchesOverThreeFrames(bool enableEnhancedTouch)
    {
        InputSystem.AddDevice<Touchscreen>();

        if (enableEnhancedTouch)
            EnhancedTouchSupport.Enable();

        Measure.Method(() =>
        {
            BeginTouch(1, new Vector2(123, 234));

            BeginTouch(2, new Vector2(234, 345), queueEventOnly: true);
            MoveTouch(1, new Vector2(345, 456), queueEventOnly: true);
            MoveTouch(1, new Vector2(456, 567), queueEventOnly: true);
            MoveTouch(1, new Vector2(567, 678), queueEventOnly: true);
            MoveTouch(1, new Vector2(789, 890), queueEventOnly: true);
            InputSystem.Update();

            EndTouch(1, new Vector2(111, 222), queueEventOnly: true);
            EndTouch(2, new Vector2(111, 222), queueEventOnly: true);
            InputSystem.Update();
        })
            .MeasurementCount(100)
            .Run();
    }

    [Test, Performance]
    [Category("Performance")]
    public void Performance_ReadEveryKey()
    {
        var keyboard = InputSystem.AddDevice<Keyboard>();

        Measure.Method(() =>
        {
            foreach (var key in keyboard.allKeys)
                key.ReadValue();
        })
            .MeasurementCount(100)
            .Run();
    }

    [Test, Performance]
    [Category("Performance")]
    [TestCase("Mouse")]
    [TestCase("Touchscreen")]
    [TestCase("Keyboard")]
    [TestCase("Gamepad")]
    public void Performance_CreateDevice(string layoutName)
    {
        // Nuke builtin precompiled layouts.
        InputControlLayout.s_Layouts.precompiledLayouts.Clear();

        Measure.Method(() => InputDevice.Build<InputDevice>(layoutName))
            .MeasurementCount(100)
            .Run();
    }

    [Test, Performance]
    [Category("Performance")]
    public void Performance_TriggerAction()
    {
        var gamepad = InputSystem.AddDevice<Gamepad>();
        var action = new InputAction(binding: "<Gamepad>/buttonSouth");
        action.Enable();

        Measure.Method(() => PressAndRelease(gamepad.buttonSouth))
            .MeasurementCount(100)
            .Run();
    }

    [Test, Performance]
    [Category("Performance")]
    public void Performance_ListenForUnpairedDeviceActivity()
    {
        // Use touchscreen as it is the most complicated device we have and thus
        // will likely incur the highest cost.
        InputSystem.AddDevice<Touchscreen>();
        ++InputUser.listenForUnpairedDeviceActivity;
        InputUser.onUnpairedDeviceUsed += (control, eventPtr) => {};

        Measure.Method(() =>
        {
            BeginTouch(1, new Vector2(123, 234));
            EndTouch(1, new Vector2(234, 345));
        })
            .MeasurementCount(100)
            .Run();
    }

    [Test, Performance]
    [Category("Performance")]
    // Layouts tested here must have precompiled versions for this test to be meaningful.
    [TestCase("Mouse")]
    [TestCase("Touchscreen")]
    [TestCase("Keyboard")]
    public void Performance_CreatePrecompiledDevice(string layoutName)
    {
        Measure.Method(() => InputDevice.Build<InputDevice>(layoutName))
            .MeasurementCount(100)
            .Run();
    }

    #if UNITY_EDITOR
    [Test]
    [Category("Performance")]
    [Ignore("TODO")]
    public void TODO_CanSaveAndRestoreSystemInLessThan10Milliseconds() // Currently it's >200ms!
    {
        Assert.Fail();
    }

    #endif
}
