# 🎲 Lost in words

> A narrative RPG where an AI Dungeon Master powered by a **local LLM** tells your story — no internet required.

![Unity](https://img.shields.io/badge/Unity-2022.3_LTS-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-.NET_Standard_2.1-purple?logo=csharp)
![LLM Unity](https://img.shields.io/badge/LLM_Unity-3.0.2-blue)
![License](https://img.shields.io/badge/license-MIT-green)

---

## 📖 About

**Lost in words** is a single-player narrative game built in Unity. A local Large Language Model acts as a Dungeon Master, generating atmospheric dark-fantasy stories in real time and reacting to every choice you make.

The game uses **D&D 5e mechanics**: every choice has a required stat (STR, DEX, CON, INT, WIS, CHA) and a Difficulty Class (DC). Roll 1d20, add your modifier — succeed or suffer the consequences.

No cloud API. No subscription. The model runs entirely on your machine.

---

## ✨ Features

- 🤖 **Generative narration** — every session is unique, powered by a local LLM
- 🎯 **D&D 5e stat checks** — DC system with real character modifiers
- ❤️ **HP & death system** — fail checks to lose HP; reach zero and it's Game Over
- 📡 **Streaming text** — narrative appears word by word as the LLM generates it
- 💾 **Persistent character** — stats loaded from `SaveData_Character.json`
- 🔒 **Fully offline** — model runs locally via [LLM Unity](https://github.com/undreamai/LLMUnity)

---

## 🏗️ Architecture

The project is organized into seven C# classes inside the `DungeonMasterAI` namespace:

| Class | Pattern | Responsibility |
|---|---|---|
| `DungeonMasterController` | Controller | Manages LLM: system prompt, streaming, JSON parsing |
| `ChoiceManager` | UI Controller | Generates buttons, runs D20 roll, applies damage |
| `PlayerStats` | Singleton | Loads stats from JSON, manages HP, fires events |
| `NarrativeUIController` | Observer | Displays streaming narrative and HP bar |
| `GameEventSystem` | Mediator | Event bus between game systems and the LLM |
| `GameEvent` | Factory | Builds contextual messages for the LLM |
| `CharacterStatsUI` | View | Displays character stats with modifier colors |

### Game loop

```
Start → DungeonMasterController injects stats into system prompt
      → LLM generates opening narration + 3 choices (JSON)
      → Player picks a choice
      → 1d20 + stat modifier vs DC
          ├─ Success → LLM narrates victory + new choices
          └─ Failure → TakeDamage() → LLM narrates consequences + new choices
                           └─ HP = 0 → Game Over scene
```

---

## 🛠️ Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| Unity | 2022.3 LTS | Game engine, UI, game loop |
| C# | .NET Standard 2.1 | Game logic, event system, JSON parsing |
| LLM Unity | 3.0.2 | Local LLM integration with streaming |
| llama.cpp (GGUF) | Q4_K_M | Inference engine |
| TextMeshPro | Built-in | Narrative text rendering with rich text |

---

## 📋 Requirements

### To run the built executable
- Windows 10/11 64-bit
- 8 GB RAM minimum (16 GB recommended for 7B models)
- ~5 GB free disk space (for the model file)
- No GPU required (CPU inference)

### To open and build from source
- Unity 2022.3 LTS
- LLM Unity package 3.0.2
- A GGUF model file (e.g. `mistral-7b-instruct-v0.2.Q4_K_M.gguf`) placed in `Assets/StreamingAssets/`

---

## 🚀 Installation & Setup

### Running the executable
1. Download the latest release from the [Releases](../../releases) page
2. Extract the zip — **keep all files together**, do not move the `.exe` out of the folder
3. Run `DungeonMasterAI.exe`
4. No configuration needed — the model is bundled inside

### Building from source
1. Clone the repository:
   ```bash
   git clone https://github.com/JavierBlascoGomez/rol_game.git
   ```
2. Open the project in **Unity 2022.3 LTS**
3. Install **LLM Unity 3.0.2** via the Package Manager
4. Download a GGUF model (e.g. Mistral 7B Instruct Q4_K_M from [HuggingFace](https://huggingface.co/TheBloke/Mistral-7B-Instruct-v0.2-GGUF)) and place it in:
   ```
   Assets/StreamingAssets/mistral-7b-instruct-v0.2.Q4_K_M.gguf
   ```
5. In the `LLMSetup` component inspector, set **Model File Name** to the exact filename
6. Add all scenes to **File → Build Settings → Scenes In Build**
7. Build: **File → Build Settings → Build**

---

## 🎮 How to Play

1. Your character has six stats (STR, DEX, CON, INT, WIS, CHA) loaded from `SaveData_Character.json`
2. The AI Dungeon Master narrates a scene and presents **3 choices**
3. Each choice shows the required stat and its DC (e.g. `Strength (+1) | DC 15`)
4. Click a choice — a **1d20 roll** happens automatically:
   - `roll + modifier ≥ DC` → **Success** — the LLM narrates your triumph
   - `roll + modifier < DC` → **Failure** — you lose 2–6 HP and the LLM narrates the consequences
5. Reach **0 HP** → Game Over

---

## 📁 Project Structure

```
rol_game/
└── Juego Rol/
    └── Assets/
        ├── Scripts/
        │   ├── DungeonMasterController.cs
        │   ├── ChoiceManager.cs
        │   ├── PlayerStats.cs
        │   ├── NarrativeUIController.cs
        │   ├── GameEventSystem.cs
        │   ├── GameEvent.cs
        │   ├── CharacterStatsUI.cs
        │   └── LLMSetup.cs
        └── StreamingAssets/
            └── mistral-7b-instruct-v0.2.Q4_K_M.gguf  ← not tracked by git (too large)
```

> **Note:** The `.gguf` model file is not included in the repository due to its size (~4 GB). Download it separately and place it in `StreamingAssets/`.

---

## 🔧 Configuration

The system prompt and opening prompt are editable directly in the Unity Inspector on the `DungeonMasterController` component. The recommended system prompt format:

```
You are a Dungeon Master for a dark fantasy RPG.
[narrative rules]
CRITICAL — OUTPUT FORMAT:
After your narration you MUST write a JSON block.
{"choices":[{"text":"...","dc":12,"stat":"STR"},{"text":"...","dc":14,"stat":"DEX"},{"text":"...","dc":10,"stat":"CHA"}]}
- Always exactly 3 choices
- Do NOT wrap it in backticks
```

---
## 👤 Authors

**Javier Blasco Gómez**  
AI and Big Data Development — 2026

**Francisco Javier Pérez Molina**
AI and Big Data Development — 2026

---

*Built with Unity + a local LLM. No cloud. No subscriptions. Just dice rolls and dark fantasy.*

# 🎲 Lost in words

> Un RPG narrativo donde un Director de Juego con **IA local** cuenta tu historia — sin necesidad de internet.

![Unity](https://img.shields.io/badge/Unity-2022.3_LTS-black?logo=unity)
![C#](https://img.shields.io/badge/C%23-.NET_Standard_2.1-purple?logo=csharp)
![LLM Unity](https://img.shields.io/badge/LLM_Unity-3.0.2-blue)
![License](https://img.shields.io/badge/licencia-MIT-green)

---

## 📖 Descripción

**Lost in words** es un juego narrativo de un solo jugador desarrollado en Unity. Un modelo de lenguaje local actúa como Director de Juego (DM), generando historias de fantasía oscura en tiempo real y reaccionando a cada decisión que tomas.

El juego usa **mecánicas de D&D 5e**: cada opción requiere una estadística (STR, DEX, CON, INT, WIS, CHA) y tiene una Clase de Dificultad (DC). Tira 1d20, suma tu modificador — supera el resultado o sufre las consecuencias.

Sin API en la nube. Sin suscripción. El modelo corre completamente en tu máquina.

---

## ✨ Características

- 🤖 **Narración generativa** — cada sesión es única, impulsada por un LLM local
- 🎯 **Tiradas de stat D&D 5e** — sistema de DC con modificadores reales del personaje
- ❤️ **Sistema de HP y muerte** — falla tiradas para perder HP; llega a cero y es Game Over
- 📡 **Texto en streaming** — la narración aparece palabra a palabra mientras el LLM genera
- 💾 **Personaje persistente** — estadísticas cargadas desde `SaveData_Character.json`
- 🔒 **Totalmente offline** — el modelo corre localmente vía [LLM Unity](https://github.com/undreamai/LLMUnity)

---

## 🏗️ Arquitectura

El proyecto está organizado en siete clases C# dentro del namespace `DungeonMasterAI`:

| Clase | Patrón | Responsabilidad |
|---|---|---|
| `DungeonMasterController` | Controller | Gestiona el LLM: system prompt, streaming, parseo JSON |
| `ChoiceManager` | UI Controller | Genera botones, ejecuta tirada D20, aplica daño |
| `PlayerStats` | Singleton | Carga stats del JSON, gestiona HP, dispara eventos |
| `NarrativeUIController` | Observer | Muestra texto narrativo con streaming y barra de HP |
| `GameEventSystem` | Mediator | Bus de eventos entre sistemas de juego y el LLM |
| `GameEvent` | Factory | Construye mensajes contextuales para el LLM |
| `CharacterStatsUI` | View | Muestra estadísticas del personaje con colores de bonificador |

### Bucle de juego

```
Inicio → DungeonMasterController inyecta stats en el system prompt
       → El LLM genera narración inicial + 3 opciones (JSON)
       → El jugador elige una opción
       → 1d20 + modificador de stat vs DC
           ├─ Éxito → El LLM narra la victoria + nuevas opciones
           └─ Fallo → TakeDamage() → El LLM narra las consecuencias + nuevas opciones
                          └─ HP = 0 → Escena Game Over
```

---

## 🛠️ Stack Tecnológico

| Tecnología | Versión | Uso |
|---|---|---|
| Unity | 2022.3 LTS | Motor de juego, UI, ciclo de juego |
| C# | .NET Standard 2.1 | Lógica de juego, sistema de eventos, parseo JSON |
| LLM Unity | 3.0.2 | Integración del LLM local con streaming |
| llama.cpp (GGUF) | Q4_K_M | Motor de inferencia |
| TextMeshPro | Integrado | Renderizado de texto narrativo con rich text |

---

## 📋 Requisitos

### Para ejecutar el juego
- Windows 10/11 64 bits
- 8 GB de RAM mínimo (16 GB recomendado para modelos 7B)
- ~5 GB de espacio libre en disco (para el archivo del modelo)
- No se requiere GPU (inferencia por CPU)

### Para abrir y compilar desde el código fuente
- Unity 2022.3 LTS
- Paquete LLM Unity 3.0.2
- Un archivo de modelo GGUF (p. ej. `mistral-7b-instruct-v0.2.Q4_K_M.gguf`) colocado en `Assets/StreamingAssets/`

---

## 🚀 Instalación

### Ejecutar el juego compilado
1. Descarga la última versión desde la página de [Releases](../../releases)
2. Extrae el zip — **mantén todos los archivos juntos**, no muevas el `.exe` fuera de la carpeta
3. Ejecuta `DungeonMasterAI.exe`
4. No se necesita ninguna configuración — el modelo va incluido

### Compilar desde el código fuente
1. Clona el repositorio:
   ```bash
   git clone https://github.com/JavierBlascoGomez/rol_game.git
   ```
2. Abre el proyecto en **Unity 2022.3 LTS**
3. Instala **LLM Unity 3.0.2** desde el Package Manager
4. Descarga un modelo GGUF (p. ej. Mistral 7B Instruct Q4_K_M desde [HuggingFace](https://huggingface.co/TheBloke/Mistral-7B-Instruct-v0.2-GGUF)) y colócalo en:
   ```
   Assets/StreamingAssets/mistral-7b-instruct-v0.2.Q4_K_M.gguf
   ```
5. En el inspector del componente `LLMSetup`, establece **Model File Name** con el nombre exacto del archivo
6. Añade todas las escenas en **File → Build Settings → Scenes In Build**
7. Compila: **File → Build Settings → Build**

---

## 🎮 Cómo se juega

1. Tu personaje tiene seis estadísticas (STR, DEX, CON, INT, WIS, CHA) cargadas desde `SaveData_Character.json`
2. El Director de Juego IA narra una escena y presenta **3 opciones**
3. Cada opción muestra la estadística requerida y su DC (p. ej. `Strength (+1) | DC 15`)
4. Haz clic en una opción — se realiza automáticamente una **tirada de 1d20**:
   - `tirada + modificador ≥ DC` → **Éxito** — el LLM narra tu triunfo
   - `tirada + modificador < DC` → **Fallo** — pierdes 2–6 HP y el LLM narra las consecuencias
5. Llega a **0 HP** → Game Over

---

## 📁 Estructura del proyecto

```
rol_game/
└── Juego Rol/
    └── Assets/
        ├── Scripts/
        │   ├── DungeonMasterController.cs
        │   ├── ChoiceManager.cs
        │   ├── PlayerStats.cs
        │   ├── NarrativeUIController.cs
        │   ├── GameEventSystem.cs
        │   ├── GameEvent.cs
        │   ├── CharacterStatsUI.cs
        │   └── LLMSetup.cs
        └── StreamingAssets/
            └── mistral-7b-instruct-v0.2.Q4_K_M.gguf  ← no incluido en git (demasiado grande)
```

> **Nota:** El archivo `.gguf` del modelo no está incluido en el repositorio por su tamaño (~4 GB). Descárgalo por separado y colócalo en `StreamingAssets/`.

---

## 🔧 Configuración

El system prompt y el opening prompt son editables directamente en el Inspector de Unity, en el componente `DungeonMasterController`. El formato de system prompt recomendado:

```
You are a Dungeon Master for a dark fantasy RPG.
[reglas narrativas]
CRITICAL — OUTPUT FORMAT:
After your narration you MUST write a JSON block.
{"choices":[{"text":"...","dc":12,"stat":"STR"},{"text":"...","dc":14,"stat":"DEX"},{"text":"...","dc":10,"stat":"CHA"}]}
- Always exactly 3 choices
- Do NOT wrap it in backticks
```

## 👤 Autores

**Javier Blasco Gómez**  
Desarrollo de Inteligencia Arficial y Big Data — 2025/2026

**Francisco Javier Pérez Molina**  
Desarrollo de Inteligencia Arficial y Big Data — 2025/2026

---

*Hecho con Unity y un LLM local. Sin nube. Sin suscripciones. Solo tiradas de dados y fantasía oscura.*
