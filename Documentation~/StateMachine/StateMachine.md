# State-Machine
***Base system to create a state framework, including an abstract class BaseState 
and an abstract class StateMachine. For using, recommend creating an intermediate abstract class State inheriting from the BaseState to add more logic.***

  ![Illustration](Images/StructureIllustration.png)
## BASESTATE INCLUDES:

### Properties
|**Property**|**Description**|
|---|---|
|StateKey| A key equivalent to each State for StateMachine managing|

### Abstract Methods
|**Method**|**Description**|
|---|---|
|EnterState| Methods called once when a State is entered|
|UpdateState|Method called every frame while in the State|
|ExitState|Methods called once when a State is exited|
|GenerateNextState|Method to generate the next state based on the current state logic.|
  
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

