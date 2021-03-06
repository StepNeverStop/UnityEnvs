# UnityEnvs

Reinforcement Learning training environments with [ML-Agents Release 6](https://github.com/Unity-Technologies/ml-agents/tree/release_6).

## How to upgrade ML-Agents?

1. Replace following folders and files from the new released ML-Agents Project:
   - `[offical ml-agents path]/Project/Assets`→`[UnityEnvs path]/Assets`
   - `[offical ml-agents path]/Project/Packages`→`[UnityEnvs path]/Packages`
   - `[offical ml-agents path]/Project/ProjectSettings`→`[UnityEnvs path]/ProjectSettings`
   - `[offical ml-agents path]/Project/Project.sln.DotSettings`→`[UnityEnvs path]/Project.sln.DotSettings`
   - `[offical ml-agents path]/com.unity.ml-agents`→`[UnityEnvs path]/Packages/com.unity.ml-agents`
   - `[offical ml-agents path]/com.unity.ml-agents.extensions`→`[UnityEnvs path]/Packages/com.unity.ml-agents.extensions`
2. Modify `[UnityEnvs path]/Packages/manifest.json`：
   - Remove the key `com.unity.ml-agents` or change the value of it from `"file: ../../com.unity.ml-agents"` to `"file: ./com.unity.ml-agents"`
   - Remove the key `com.unity.ml-agents.extensions` or change the value of it from `"file: ../../com.unity.ml-agents.extensions"` to `"file: ./com.unity.ml-agents.extensions"`

## How to create a custom environment?

Create a unique folder in `[UnityEnvs path]/Assets/Environments` and then implement your own training environment following the guide in ml-agents.

