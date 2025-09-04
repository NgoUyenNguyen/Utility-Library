# State-Machine
***Base system to create a state framework, including an abstract class BaseState 
and an abstract class StateMachine. For using, recommend creating an intermediate abstract class State inheriting from the BaseState to add more logic.***

---
## BASESTATE INCLUDES:

### Properties
|**Property**|**Description**|
|---|---|
|[stateKey](StateKey.md)| A key equivalent to each State for StateManager managing|

### Abstract Methods
|**Method**|**Description**|
|---|---|
|[EnterState](EnterState.md)| Methods called once when a State is entered|
|[UpdateState](UpdateState.md)|Method called every frame while in the State|
|[ExitState](ExitState.md)|Methods called once when a State is exited|
|[GenerateNextState](GenerateNextState.md)|Method to generate the next state based on the current state logic.|
  
---
## STATEMANAGER INCLUDES:

### Properties
|**Property**|**Description**|
|---|---|
|CurrentState| The current state of the state machine|

### Events
|**Event**|**Description**|
|---|---|
|OnStateChanged| Callback fired whenever state changes|

### Public Methods
|**Method**|**Description**|
|---|---|
|AddStates|Add states to the state machine|
|RemoveStates|Remove states from the state machine|
|GetState|Get state from key|

### Abstract Methods
|**Method**|**Description**|
|---|---|
|InitializeStates|register all states by using AddStates|
|InitializeEntryState|Define the entry state|

### Virtual Methods
|**Method**|**Description**|
|---|---|
|OnAwake|Method is called in Awake|
|OnStart|Method is called in Start|

