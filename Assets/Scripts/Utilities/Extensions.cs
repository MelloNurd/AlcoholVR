using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Android;
using UnityEngine.Rendering;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public static class Extensions
{
    // Created by: Ben Bonus
    // This is a collection of extension  methods made for using within the Unity Game Engine.
    // These are here simply to make code a little easier to read and write. You can use them as you would with any other function on the specified type.

    #region Misc Extensions
    public static Color GetColor(this Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return Color.white;
            case Rarity.Uncommon:
                return Color.green;
            case Rarity.Rare:
                return Color.blue;
            case Rarity.Epic:
                return Color.magenta;
            case Rarity.Legendary:
                return Color.yellow;
            default:
                return Color.white;
        }
    }
    #endregion

    #region String Extensions
    /// <summary>
    /// Gets the first x characters of a string.
    /// </summary>
    /// <param name="count">The number of characters to get</param>
    /// <returns>The first x characters of a string</returns>
    public static string GetFirst(this string text, int count)
    {
        return text.Substring(0, count);
    }

    public static char GetFirst(this string text)
    {
        if (string.IsNullOrEmpty(text)) return default(char);
        return text[0];
    }

    /// <summary>
    /// Gets the last x characters of a string.
    /// </summary>
    /// <param name="count">The number of characters to get</param>
    /// <returns>The last x characters of a string</returns>
    public static string GetLast(this string text, int count)
    {
        return text.Substring(text.Length - count, count);
    }

    public static char GetLast(this string text)
    {
        if (string.IsNullOrEmpty(text)) return default(char);
        return text[text.Length - 1];
    }

    /// <summary>
    /// Returns true if the string is null or empty.
    /// </summary>
    public static bool IsBlank(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Removes all non-digit from a string and returns the result.
    /// </summary>
    /// <returns>The new digit-only string</returns>
    public static string OnlyDigits(this string value)
    {
        return new string(value?.Where(c => char.IsDigit(c)).ToArray());
    }

    /// <summary>
    /// Removes all non-letter characters from a string and returns the result.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The new letter-only string</returns>
    public static string OnlyLetters(this string value)
    {
        return new string(value?.Where(c => char.IsLetter(c)).ToArray());
    }

    /// <summary>
    /// Removes all non-letter and non-digit characters from a string and returns the result.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>The new letter-and-digit-only string</returns>
    public static string OnlyLettersAndDigits(this string value)
    {
        return new string(value?.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }

    /// <summary>
    /// Takes a string with camelCase or PascalCase and adds spaces between the words.
    /// </summary>
    /// <param name="excludeFirstChar">Set this to false to also add a space at the beginning if the first char is capitalized.</param>
    /// <returns>The new string with spaces between each word.</returns>
    public static string AsSentence(this string value, bool excludeFirstChar)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var newText = new StringBuilder(value.Length * 2);
        newText.Append(value[0]);

        int startIndex = excludeFirstChar ? 1 : 0;
        for (int i = startIndex; i < value.Length; i++)
        {
            if (char.IsUpper(value[i]) && !char.IsWhiteSpace(value[i - 1]))
            {
                newText.Append(' ');
            }
            newText.Append(value[i]);
        }

        return newText.ToString();
    }
    /// <summary>
    /// Takes a string with camelCase or PascalCase and adds spaces between the words.
    /// </summary>
    /// <returns>The new string with spaces between each word.</returns>
    public static string AsSentence(this string value) { return AsSentence(value, true); }

    /// <summary>
    /// Converts a string into a file name friendly string by replacing invalid characters with a dash.
    /// </summary>
    /// <returns>A file-name-friendly version of the string</returns>
    public static string FileNameFriendly(this string value) => FileNameFriendly(value, '-');

    /// <summary>
    /// Converts a string into a file name friendly string by replacing invalid characters with a chosen character.
    /// </summary>
    /// <param name="replacementCharacter">Character to replace any invalid characters</param>
    /// <returns>A file-name-friendly version of the string</returns>
    public static string FileNameFriendly(this string value, char replacementCharacter, bool replaceSpaces = true)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char invalidChar in invalidChars)
        {
            value = value.Replace(invalidChar, replacementCharacter);
        }

        value = value.TrimEnd('.'); // Remove trailing dots

        // Replace spaces with underscores for better readability
        return replaceSpaces ? value.Replace(' ', replacementCharacter).Trim() : value.Trim();
    }

    /// <summary>
    /// Removes all control characters from a string, leaving only printable characters.
    /// </summary>
    /// <returns>The new version of the string with no control characters.</returns>
    public static string Cleaned(this string value)
    {
        StringBuilder builder = new();

        foreach (char c in value)
        {
            // Remove control characters AND format characters (includes zero-width spaces and other invisible Unicode)
            if (char.IsControl(c) || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.Format) 
                continue;

            //Debug.Log(c);

            builder.Append(c);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Automatically increments the file name number if there are existing files with the same name.
    /// </summary>
    /// <returns>The new, adjusted filePath. If no file with the same name exists, the original string.</returns>
    public static string AutoIncrementFileName(this string filePath)
    {
        if (!File.Exists(filePath)) return filePath;

        // Check for number at end of filePath. If has, increment and concat. Otherwise, add a 1 at the end.
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string fileExtension = Path.GetExtension(filePath);
        string directory = Path.GetDirectoryName(filePath);
        string newFilePath = Path.Combine(directory, fileName + fileExtension);
        int i = 1;
        while (File.Exists(newFilePath))
        {
            string newFileName = $"{fileName} ({i})";
            newFilePath = Path.Combine(directory, newFileName + fileExtension);
            i++;
        }
        return newFilePath;
    }

    #endregion

    #region Float Extensions

    /// <summary>
    /// Rounds a float to the nearest whole number.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>-1 or 1, based on the sign of the float.</returns>
    public static float Sign(this float value)
    {
        return value >= 0 ? 1f : -1f;
    }
    /// <summary>
    /// Rounds a float to the nearest whole number, including zero.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>-1, 0, 1, based on the sign of the float. A value of zero will still return zero.</returns>
    public static float SignZeroed(this float value)
    {
        return value > 0 ? 1f : value < 0 ? -1f : 0f;
    }

    /// <summary>
    /// Converts a floating-point value representing seconds to an integer value representing milliseconds.
    /// </summary>
    /// <param name="value">The time duration in seconds to be converted to milliseconds.</param>
    /// <returns>An integer representing the equivalent time duration in milliseconds.</returns>
    public static int ToMS(this float value)
    {
        return Mathf.RoundToInt(value * 1000f);
    }

    #endregion

    #region Int Extensions

    /// <summary>
    /// Rounds an int to the nearest whole number.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>-1 or 1, based on the sign of the int.</returns>
    public static int Sign(this int value)
    {
        return value >= 0 ? 1 : -1;
    }
    /// <summary>
    /// Rounds an int to the nearest whole number, including zero.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>-1, 0, 1, based on the sign of the int. A value of zero will still return zero.</returns>
    public static int SignZeroed(this int value)
    {
        return value > 0 ? 1 : value < 0 ? -1 : 0;
    }

    #endregion

    #region List Extensions
    /// <summary>
    /// Gets a random value from the list
    /// </summary>
    /// <returns>A random value from the list, or the default value for an empty list.</returns>
    public static T GetRandom<T>(this IList<T> list)
    {
        if (list.Count == 0)
        {
            return default(T);
        }
        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Gets a random value from the list that is different from a previous item.
    /// </summary>
    /// <param name="previousItem">The item to avoid returning</param>
    /// <returns>A random value from the list different from the previous item, or the default value if the list is empty.</returns>
    public static T GetRandomUnique<T>(this IList<T> list, T previousItem)
    {
        if (previousItem == null)
        {
            return list.GetRandom(); // If previousItem is null, just return a random item
        }

        if (list.Count == 0)
        {
            return default(T);
        }

        if (list.Count == 1)
        {
            return list[0]; // Only one option available
        }

        int randomIndex;
        T randomItem;
        int attempts = 0;
        int maxAttempts = 20; // Prevent infinite loop if all items are equal

        do
        {
            randomIndex = Random.Range(0, list.Count);
            randomItem = list[randomIndex];
            attempts++;
        }
        while (EqualityComparer<T>.Default.Equals(randomItem, previousItem) && attempts < maxAttempts);

        return randomItem;
    }

    /// <summary>
    /// Destroys all MonoBehaviour objects in the list and clears the list.
    /// </summary>
    /// <returns>The number of objects destroyed</returns>
    public static int DestroyAllAndClear<T>(this IList<T> List) where T : MonoBehaviour
    {
        int temp = List.Count;
        foreach (var item in List)
        {
            if (item != null)
            {
                GameObject.Destroy(item.gameObject);
            }
        }
        List.Clear();
        return temp;
    }
    #endregion

    #region Dictonary Extensions
    /// <summary>
    /// Reverses the keys and values of a dictionary.
    /// </summary>
    /// <returns>A new dictonary with the keys and values swapped.</returns>
    public static Dictionary<TValue, TKey> Reverse<TKey, TValue>(this IDictionary<TKey, TValue> source)
    {
        var dictionary = new Dictionary<TValue, TKey>();
        foreach (var entry in source)
        {
            if (!dictionary.ContainsKey(entry.Value))
                dictionary.Add(entry.Value, entry.Key);
        }
        return dictionary;
    }

    /// <summary>
    /// Finds the key associated with the given value in a dictionary.
    /// </summary>
    /// <param name="lookup">The value to lookup</param>
    /// <returns>The key associating with the given lookup value</returns>
    public static TKey ReverseLookup<TKey, TValue>(this IDictionary<TKey, TValue> source, TValue lookup) where TValue : class
    {
        return source.FirstOrDefault(x => x.Value == lookup).Key;
    }

    ////////// SERIALIZED DICTIONARIES ////////////

    /// <summary>
    /// Clones a dictionary by creating a new dictionary and copying the keys and values.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="original"></param>
    /// <returns>An exact copy of the original Dictionary.</returns>
    public static SerializedDictionary<TKey, TValue> Clone<TKey, TValue>(this SerializedDictionary<TKey, TValue> original)
    {
        var copy = new SerializedDictionary<TKey, TValue>();
        foreach (var kvp in original)
        {
            copy.Add(kvp.Key, kvp.Value); // Consider deep-copying TValue if it's a reference type
        }
        return copy;
    }
    #endregion

    #region Vector3 Extensions
    /// <summary>
    /// Returns a new Vector3 with the x value changed to the given value.
    /// </summary>
    /// <param name="x">The new value for x</param>
    /// <returns>The vector3 with the new x value</returns>
    public static Vector3 WithX(this Vector3 vector, float x)
    {
        vector.x = x;
        return vector;
    }
    /// <summary>
    /// Returns a new Vector3 with the y value changed to the given value.
    /// </summary>
    /// <param name="y">The new value for y</param>
    /// <returns>The vector3 with the new y value</returns>
    public static Vector3 WithY(this Vector3 vector, float y)
    {
        vector.y = y;
        return vector;
    }
    /// <summary>
    /// Returns a new Vector3 with the z value changed to the given value.
    /// </summary>
    /// <param name="z">The new value for z</param>
    /// <returns>The vector3 with the new z value</returns>
    public static Vector3 WithZ(this Vector3 vector, float z)
    {
        vector.z = z;
        return vector;
    }

    /// <summary>
    /// Returns a new Vector3 with the x value increased by the given amount.
    /// </summary>
    /// <param name="x">The amount to add to x</param>
    /// <returns>The vector3 with the increased x value</returns>
    public static Vector3 AddX(this Vector3 vector, float x)
    {
        vector.x += x;
        return vector;
    }
    /// <summary>
    /// Returns a new Vector3 with the y value increased by the given amount.
    /// </summary>
    /// <param name="y">The amount to add to y</param>
    /// <returns>The vector3 with the increased y value</returns>
    public static Vector3 AddY(this Vector3 vector, float y)
    {
        vector.y += y;
        return vector;
    }
    /// <summary>
    /// Returns a new Vector3 with the z value increased by the given amount.
    /// </summary>
    /// <param name="z">The amount to add to z</param>
    /// <returns>The vector3 with the increased z value</returns>
    public static Vector3 AddZ(this Vector3 vector, float z)
    {
        vector.z += z;
        return vector;
    }

    /// <summary>
    /// Converts a Vector3 to a Vector2 by removing the z value.
    /// </summary>
    /// <returns>A vector2 made with the x and y of the vector3</returns>
    public static Vector2 ToVector2(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }

    /// <summary>
    /// Clamps a Vector3 between the given min and max value bounds.
    /// </summary>
    /// <param name="min">The minimum value allowed</param>
    /// <param name="max">The maximum value allowed</param>
    /// <returns>A Vector3 guaranteed to be within the given bounds</returns>
    public static Vector3 Clamp(this Vector3 value, float min, float max)
    {
        value.x = Mathf.Clamp(value.x, min, max);
        value.y = Mathf.Clamp(value.y, min, max);
        value.z = Mathf.Clamp(value.z, min, max);

        return value;
    }

    /// <summary>
    /// Clamps a Vector3 between the given min and max vector bounds.
    /// </summary>
    /// <param name="min">The minimum vector allowed</param>
    /// <param name="max">The maximum vector allowed</param>
    /// <returns>A Vector3 guaranteed to be within the given bounds</returns>
    public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
    {
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);
        value.z = Mathf.Clamp(value.z, min.z, max.z);

        return value;
    }
    #endregion

    #region Vector2 Extensions
    /// <summary>
    /// Returns a new Vector2 with the x value changed to the given value.
    /// </summary>
    /// <param name="x">The new value for x</param>
    /// <returns>The vector2 with the new x value</returns>
    public static Vector2 WithX(this Vector2 vector, float x)
    {
        vector.x = x;
        return vector;
    }
    /// <summary>
    /// Returns a new Vector2 with the y value changed to the given value.
    /// </summary>
    /// <param name="y">The new value for y</param>
    /// <returns>The vector2 with the new y value</returns>
    public static Vector2 WithY(this Vector2 vector, float y)
    {
        vector.y = y;
        return vector;
    }

    /// <summary>
    /// Returns a new Vector2 with the x value increased by the given amount.
    /// </summary>
    /// <param name="x">The amount to add to x</param>
    /// <returns>The vector2 with the increased x value</returns>
    public static Vector2 AddX(this Vector2 vector, float x)
    {
        vector.x += x;
        return vector;
    }
    /// <summary>
    /// Returns a new Vector2 with the y value increased by the given amount.
    /// </summary>
    /// <param name="y">The amount to add to y</param>
    /// <returns>The vector2 with the increased y value</returns>
    public static Vector2 AddY(this Vector2 vector, float y)
    {
        vector.y += y;
        return vector;
    }

    /// <summary>
    /// Converts a Vector2 to a Vector3 by adding a z value of 0.
    /// </summary>
    /// <returns>A new Vector3 with the x and y of the Vector2 and a z of 0.</returns>
    public static Vector3 ToVector3(this Vector2 vector)
    {
        return new Vector3(vector.x, vector.y, 0);
    }

    /// <summary>
    /// Converts a Vector2 to a Vector3 by adding the given z value.
    /// </summary>
    /// <param name="z">The new value for z</param>
    /// <returns>A new Vector3 with the x and y of the Vector2 and the given value for z.</returns>
    public static Vector3 ToVector3(this Vector2 vector, int z)
    {
        return new Vector3(vector.x, vector.y, z);
    }

    /// <summary>
    /// Clamps a Vector2 between the given min and max value bounds.
    /// </summary>
    /// <param name="min">The minimum value allowed</param>
    /// <param name="max">The maximum value allowed</param>
    /// <returns>A Vector2 guaranteed to be within the given bounds</returns>
    public static Vector2 Clamp(this Vector2 value, float min, float max)
    {
        value.x = Mathf.Clamp(value.x, min, max);
        value.y = Mathf.Clamp(value.y, min, max);

        return value;
    }

    /// <summary>
    /// Clamps a Vector2 between the given min and max vector bounds.
    /// </summary>
    /// <param name="min">The minimum vector allowed</param>
    /// <param name="max">The maximum vector allowed</param>
    /// <returns>A Vector2 guaranteed to be within the given bounds</returns>
    public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max)
    {
        value.x = Mathf.Clamp(value.x, min.x, max.x);
        value.y = Mathf.Clamp(value.y, min.y, max.y);

        return value;
    }
    #endregion

    #region GameObject Extensions

    /// <summary>
    /// Gets a component of the given type attached to the GameObject. If that type of component does not exist, it adds one.
    /// </summary>
    /// <typeparam name="T">The type of the component to get or add.</typeparam>
    /// <param name="gameObject">The GameObject to get the component from or add the component to.</param>
    /// <returns>The existing component of the given type, or a new one if no such component exists.</returns>    
    public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (!component) component = gameObject.AddComponent<T>();

        return component;
    }

    #endregion

    #region Transform Extensions
    /// <summary>
    /// Sets position and rotation to zero and scale to one.
    /// </summary>
    public static void Reset(this Transform transform)
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
    /// <summary>
    /// Sets local position and rotation to zero and scale to one.
    /// </summary>
    public static void LocalReset(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Executes a specified action for each child of a given transform.
    /// </summary>
    /// <param name="parent">The parent transform.</param>
    /// <param name="action">The action to be performed on each child.</param>
    /// <remarks>
    /// This method iterates over all child transforms in reverse order and executes a given action on them.
    /// The action is a delegate that takes a Transform as parameter.
    /// </remarks>
    public static void ForEveryChild(this Transform parent, System.Action<Transform> action)
    {
        for (var i = parent.childCount - 1; i >= 0; i--)
        {
            action(parent.GetChild(i));
        }
    }
    #endregion

    #region Color Extensions

    /// <summary>
    /// Applies an alpha value to a color, clamping it between 0 and 1.
    /// </summary>
    /// <param name="alpha">The value to set the alpha to, clampped between 0 and 1.</param>
    /// <returns>The color with the new alpha value</returns>
    public static Color WithAlpha(this Color color, float alpha)
    {
        color.a = Mathf.Clamp01(alpha); // Ensure alpha is between 0 and 1
        return color;
    }

    #endregion

    #region CanvasGroup Extensions
    /// <summary>
    /// Enables alpha and interaction on a CanvasGroup.
    /// </summary>
    public static void Show(this CanvasGroup group)
    {
        group.interactable = true;
        group.blocksRaycasts = true;
        group.alpha = 1f;
        group.ignoreParentGroups = false;
    }

    /// <summary>
    /// Disables alpha and interaction on a CanvasGroup.
    /// </summary>
    public static void Hide(this CanvasGroup group)
    {
        group.interactable = false;
        group.blocksRaycasts = false;
        group.alpha = 0f;
        group.ignoreParentGroups = true;
    }

    /// <summary>
    /// Whether or not the CanvasGroup is visible.
    /// </summary>
    /// <param name="group"></param>
    /// <returns>A bool representing the visibility state of the canvas group.</returns>
    public static bool IsVisible(this CanvasGroup group)
    {
        return group.alpha > 0f;
    }
    #endregion

    #region NavMeshAgent Extensions

    /// <summary>
    /// Checks if the NavMeshAgent has reached its destination within a specified threshold.
    /// </summary>
    /// <param name="agent">The NavMeshAgent to check</param>
    /// <param name="threshold">The distance threshold to consider as "at destination" (default: 0.1f)</param>
    /// <returns>True if the agent is at its destination, false otherwise</returns>
    public static bool IsAtDestination(this UnityEngine.AI.NavMeshAgent agent, float threshold = 0.1f)
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= threshold)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the NavMeshAgent is currently moving based on its velocity.
    /// </summary>
    /// <param name="agent">The NavMeshAgent to check</param>
    /// <param name="threshold">The velocity threshold to consider as "moving" (default: 0.1f)</param>
    /// <returns>True if the agent is moving, false otherwise</returns>
    public static bool IsMoving(this UnityEngine.AI.NavMeshAgent agent, float threshold = 0.1f)
    {
        return agent.velocity.sqrMagnitude > threshold * threshold;
    }

    /// <summary>
    /// Checks if the NavMeshAgent has a valid and complete path.
    /// </summary>
    /// <param name="agent">The NavMeshAgent to check</param>
    /// <returns>True if the agent has a complete path, false otherwise</returns>
    public static bool HasPath(this UnityEngine.AI.NavMeshAgent agent)
    {
        return agent.hasPath && agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathComplete;
    }

    /// <summary>
    /// Checks if the NavMeshAgent is stopped or not moving.
    /// </summary>
    /// <param name="agent">The NavMeshAgent to check</param>
    /// <returns>True if the agent is stopped, false otherwise</returns>
    public static bool IsStopped(this UnityEngine.AI.NavMeshAgent agent)
    {
        return agent.isStopped || !agent.hasPath || agent.velocity.sqrMagnitude == 0f;
    }

    /// <summary>
    /// Stops the NavMeshAgent and makes it face towards a target position.
    /// </summary>
    /// <param name="agent">The NavMeshAgent to stop and rotate</param>
    /// <param name="targetPosition">The position to face towards</param>
    public static void StopAndFace(this UnityEngine.AI.NavMeshAgent agent, Vector3 targetPosition)
    {
        agent.isStopped = true;
        Vector3 direction = (targetPosition - agent.transform.position).WithY(0).normalized;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// Sets the destination to the closest valid point on the NavMesh near the target destination.
    /// This avoids picking points on different vertical floors by enforcing a max vertical difference.
    /// Added optional visual debug parameters to show sampling and decisions.
    /// </summary>
    /// <returns>The newly found position on the NavMesh.</returns>
    public static bool SetDestinationToClosestPoint(this UnityEngine.AI.NavMeshAgent agent, Vector3 targetDestination, float checkRadius = 1.25f, bool startAtGround = true, bool limitHeight = true)
    {
        Vector3 result = GetNearestPointOnNavMesh(targetDestination, checkRadius, startAtGround, limitHeight);

        if (result == Vector3.zero)
        {
            Debug.LogWarning($"No valid NavMesh point found near target destination. Not setting position for {agent.gameObject.name}.");
            return false;
        }

        agent.SetDestination(result);
        return true;
    }

    public static Vector3 GetNearestPointOnNavMesh(Vector3 targetDestination, float checkRadius = 1.25f, bool startOnGround = true, bool limitHeight = true)
    {
        Vector3 result = targetDestination;

        if (startOnGround)
        {
            if (Physics.Raycast(result, Vector3.down, out RaycastHit info, 5f))
            {
                result = info.point;
            }
        }

        int checks = 80;
        for (int i = 0; i < checks; i++)
        {
            float percent = (i + 1) / (checks / 2f); // Goes from 0 to 2 (so checkRadius doubles at the end)

            Vector3 randomOffsetInSphere = Random.insideUnitSphere;
            if(limitHeight) randomOffsetInSphere.y = Mathf.Clamp(randomOffsetInSphere.y, -0.5f, 0.5f); // Keep it flat on the XZ plane

            Vector3 randomPoint = result + (percent * checkRadius * randomOffsetInSphere); // Basically, increase radius over time
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                DrawSphere(randomPoint, 0.25f, Color.green, duration: 10f);
                result = hit.position;
                Debug.Log($"Found position ({result}) in {i + 1} attempts.");
                return result;
            }
            else
            {
                DrawSphere(randomPoint, 0.25f, Color.red, duration: 10f);
            }
        }

        return Vector3.zero;
    }

    // --- Debug helpers (XZ-plane visualizations using Debug.DrawLine) ---
    private static void DrawCross(Vector3 position, Color color, float size = 0.1f, float duration = 0f)
    {
        Vector3 up = Vector3.up * (size * 0.1f);
        Vector3 a = position + new Vector3(size, 0, 0) + up;
        Vector3 b = position + new Vector3(-size, 0, 0) + up;
        Vector3 c = position + new Vector3(0, 0, size) + up;
        Vector3 d = position + new Vector3(0, 0, -size) + up;
        Debug.DrawLine(a, b, color, duration);
        Debug.DrawLine(c, d, color, duration);

        // small vertical to indicate height
        Debug.DrawLine(position, position + Vector3.up * (size * 0.5f), color, duration);
    }

    private static void DrawCircleXZ(Vector3 center, float radius, Color color, int segments = 24, float duration = 0f)
    {
        if (segments < 4) segments = 4;
        float step = (Mathf.PI * 2f) / segments;
        Vector3 prev = center + new Vector3(Mathf.Cos(0f) * radius, 0f, Mathf.Sin(0f) * radius);
        for (int i = 1; i <= segments; i++)
        {
            float a = step * i;
            Vector3 next = center + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            Debug.DrawLine(prev, next, color, duration);
            prev = next;
        }
    }

    /// <summary>
    /// Draws an approximate wireframe sphere using Debug.DrawLine.
    /// </summary>
    /// <param name="center">Sphere center in world space.</param>
    /// <param name="radius">Sphere radius.</param>
    /// <param name="color">Color for debug lines.</param>
    /// <param name="segments">Segments per circle (horizontal / vertical resolution).</param>
    /// <param name="latitudeRings">How many horizontal rings (parallel to XZ) to draw across the Y axis (includes top/bottom if >1).</param>
    /// <param name="meridians">How many meridian lines (longitudinal) to draw.</param>
    /// <param name="duration">Debug duration.</param>
    private static void DrawSphere(Vector3 center, float radius, Color color, int segments = 24, int latitudeRings = 6, int meridians = 8, float duration = 0f)
    {
        if (radius <= 0f) return;
        segments = Mathf.Max(4, segments);
        latitudeRings = Mathf.Max(1, latitudeRings);
        meridians = Mathf.Max(0, meridians);

        // Helper to draw a circle in an arbitrary plane defined by two orthonormal axes
        System.Action<Vector3, Vector3, Vector3, float, Color> DrawCircle = (c, axisA, axisB, r, col) =>
        {
            float step = (Mathf.PI * 2f) / segments;
            Vector3 prev = c + (axisA * Mathf.Cos(0f) + axisB * Mathf.Sin(0f)) * r;
            for (int i = 1; i <= segments; i++)
            {
                float a = step * i;
                Vector3 next = c + (axisA * Mathf.Cos(a) + axisB * Mathf.Sin(a)) * r;
                Debug.DrawLine(prev, next, col, duration);
                prev = next;
            }
        };

        // Great circles: XZ, XY, YZ
        DrawCircle(center, Vector3.right, Vector3.forward, radius, color); // XZ (same as DrawCircleXZ)
        DrawCircle(center, Vector3.right, Vector3.up, radius, color);      // XY
        DrawCircle(center, Vector3.up, Vector3.forward, radius, color);    // YZ

        // Latitudinal rings (parallel to XZ) at several Y heights to give sphere "thickness"
        if (latitudeRings > 1)
        {
            for (int i = 0; i < latitudeRings; i++)
            {
                float t = i / (float)(latitudeRings - 1); // 0..1
                float y = Mathf.Lerp(-radius, radius, t);
                float ringRadius = Mathf.Sqrt(Mathf.Max(0f, radius * radius - y * y));
                Vector3 ringCenter = center + Vector3.up * y;
                // Slightly dim color for inner rings
                Color ringColor = color * 0.9f;
                DrawCircle(ringCenter, Vector3.right, Vector3.forward, ringRadius, ringColor);
            }
        }

        // Meridians: lines from bottom to top along constant longitude
        // For each meridian, step polar angle from -pi/2 to +pi/2 and connect points
        if (meridians > 0)
        {
            int polarSteps = Mathf.Max(6, segments / 2); // vertical resolution along a meridian
            for (int m = 0; m < meridians; m++)
            {
                float lon = (m / (float)meridians) * Mathf.PI * 2f;
                Vector3 prev = Vector3.zero;
                bool havePrev = false;
                for (int p = 0; p <= polarSteps; p++)
                {
                    float t = p / (float)polarSteps; // 0..1
                    float polar = Mathf.Lerp(-Mathf.PI * 0.5f, Mathf.PI * 0.5f, t); // -90 to +90 deg
                    float cosP = Mathf.Cos(polar);
                    float sinP = Mathf.Sin(polar);
                    // Spherical coordinates to Cartesian
                    Vector3 point = new Vector3(Mathf.Cos(lon) * cosP, sinP, Mathf.Sin(lon) * cosP) * radius;
                    point += center;
                    if (havePrev)
                    {
                        Debug.DrawLine(prev, point, color * 0.95f, duration);
                    }
                    prev = point;
                    havePrev = true;
                }
            }
        }
    }
    #endregion  
}