# Input

## Buttons

### InputMainButton

* bool button0
* bool button1
* bool button2
* bool button3

各ボタンの押下状態。

&nbsp;

### InputSideButton

* bool bumperL
* bool bumperR
* bool triggerL
* bool triggerR
* bool stickL
* bool stickR

各ボタンの押下状態。

&nbsp;

### InputPressedEvent

* InputButtonEventKey key

ボタンのプレス時にイベントとして発行される。

|Button|説明|
|:-:|:--|
|Button0| ボタン0 |
|Button1| ボタン1 |
|Button2| ボタン2 |
|Button3| ボタン3 |
|BumperL| 左バンパーボタン|
|BumperR| 右バンパーボタン |
|TriggerL| 左トリガーボタン |
|TriggerR| 右トリガーボタン |
|StickL| 左スティックボタン |
|StickR| 右スティックボタン |
|Start| スタートボタン |
|Select| セレクトボタン |

&nbsp;

### InputReleasedEvent

* InputButtonEventKey key

ボタンのリリース時にイベントとして発行される。

&nbsp;

## Sticks

### InputMainStick

* float2 value

左スティックの入力情報。

&nbsp;

### InputSubStick

* float2 value

右スティックの入力情報。

&nbsp;

[back](./_.md)