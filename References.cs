using System;
using System.Collections.Generic;
using System.Linq;

// 1. Interface for Parameter-Aware Members
public interface IParameterAware
{
    string Name { get; }
    string GetExpression(); // e.g., address expression or offset
}

// 2. Parameter Class
public class Parameter
{
    public string Name { get; set; }
    public string Value { get; set; }
}

// 3. Register Class
public class Register : IParameterAware
{
    public string Name { get; set; }
    public string AddressExpression { get; set; }

    public string GetExpression() => AddressExpression;
}

// 4. Interface Class (example of reuse)
public class Interface : IParameterAware
{
    public string Name { get; set; }
    public string OffsetExpression { get; set; }

    public string GetExpression() => OffsetExpression;
}

// 5. Component Definition
public class Component
{
    public string Name { get; set; }
    public List<Parameter> Parameters { get; } = new();
    public List<Register> Registers { get; } = new();
    public List<Interface> Interfaces { get; } = new();

    public Parameter? GetParameterByName(string name) =>
        Parameters.FirstOrDefault(p => p.Name == name);
}

// 6. ComponentInstance
public class ComponentInstance
{
    public string InstanceName { get; set; }
    public Component Definition { get; set; }

    private readonly Dictionary<string, string> OverriddenParameters = new();

    public ComponentInstance(string instanceName, Component definition)
    {
        InstanceName = instanceName;
        Definition = definition;
    }

    public void OverrideParameter(string name, string value)
    {
        if (!Definition.Parameters.Any(p => p.Name == name))
            throw new ArgumentException($"Parameter '{name}' not found in component '{Definition.Name}'");

        OverriddenParameters[name] = value;
    }

    public string GetParameterValue(string name)
    {
        if (OverriddenParameters.TryGetValue(name, out var overriddenValue))
            return overriddenValue;

        var param = Definition.GetParameterByName(name);
        return param?.Value ?? throw new KeyNotFoundException($"Parameter '{name}' not found.");
    }

    public Dictionary<string, string> GetResolvedExpressions(IEnumerable<IParameterAware> members)
    {
        var paramMap = Definition.Parameters.ToDictionary(p => p.Name, p => GetParameterValue(p.Name));
        var resolver = new ExpressionResolver(paramMap);
        return resolver.ResolveAll(members);
    }

    public Dictionary<string, string> GetResolvedRegisterAddresses() =>
        GetResolvedExpressions(Definition.Registers);

    public Dictionary<string, string> GetResolvedInterfaceOffsets() =>
        GetResolvedExpressions(Definition.Interfaces);
}

// 7. Expression Resolver
public class ExpressionResolver
{
    private readonly Dictionary<string, string> _parameters;

    public ExpressionResolver(Dictionary<string, string> parameters)
    {
        _parameters = parameters;
    }

    public string Resolve(string expression)
    {
        string result = expression;
        foreach (var kvp in _parameters)
        {
            result = result.Replace(kvp.Key, kvp.Value);
        }
        return result;
    }

    public Dictionary<string, string> ResolveAll(IEnumerable<IParameterAware> items)
    {
        return items.ToDictionary(
            item => item.Name,
            item => Resolve(item.GetExpression())
        );
    }
}

// 8. SystemSpec
public class SystemSpec
{
    public string Name { get; set; }
    public List<ComponentInstance> ComponentInstances { get; } = new();

    public void AddComponentInstance(ComponentInstance instance) =>
        ComponentInstances.Add(instance);
}

// 9. Demo Program
public static class Program
{
    public static void Main()
    {
        // Create component definition
        var uart = new Component
        {
            Name = "UART"
        };
        uart.Parameters.Add(new Parameter { Name = "BASE", Value = "0x1000" });
        uart.Registers.Add(new Register { Name = "CTRL", AddressExpression = "BASE + 0x0" });
        uart.Registers.Add(new Register { Name = "DATA", AddressExpression = "BASE + 0x4" });
        uart.Interfaces.Add(new Interface { Name = "APB", OffsetExpression = "BASE + 0x8" });

        // Create system
        var system = new SystemSpec { Name = "MyChip" };

        var inst1 = new ComponentInstance("UART0", uart);
        var inst2 = new ComponentInstance("UART1", uart);
        inst2.OverrideParameter("BASE", "0x2000");

        system.AddComponentInstance(inst1);
        system.AddComponentInstance(inst2);

        Console.WriteLine("Resolved Register Addresses for UART1:");
        foreach (var reg in inst2.GetResolvedRegisterAddresses())
        {
            Console.WriteLine($"  {reg.Key}: {reg.Value}");
        }

        Console.WriteLine("Resolved Interface Offsets for UART1:");
        foreach (var iface in inst2.GetResolvedInterfaceOffsets())
        {
            Console.WriteLine($"  {iface.Key}: {iface.Value}");
        }
    }
}




//OCP 
public interface IParameterAware
{
    string Name { get; }
    string GetExpression(); // e.g., "BASE + 0x4"
}

public class Component
{
    public string Name { get; set; }
    public List<Parameter> Parameters { get; } = new();

    // Component knows its parameterized members
    public Dictionary<string, List<IParameterAware>> ParameterizedMembers { get; } = new();

    public void AddMember<T>(T member) where T : IParameterAware
    {
        string typeName = typeof(T).Name;

        if (!ParameterizedMembers.ContainsKey(typeName))
            ParameterizedMembers[typeName] = new List<IParameterAware>();

        ParameterizedMembers[typeName].Add(member);
    }

    public IEnumerable<T> GetMembersOfType<T>() where T : IParameterAware
    {
        string typeName = typeof(T).Name;

        if (ParameterizedMembers.TryGetValue(typeName, out var members))
            return members.Cast<T>();

        return Enumerable.Empty<T>();
    }

    public Parameter? GetParameterByName(string name) =>
        Parameters.FirstOrDefault(p => p.Name == name);
}

public class ComponentInstance
{
    public string InstanceName { get; set; }
    public Component Definition { get; set; }
    private readonly Dictionary<string, string> OverriddenParameters = new();

    public ComponentInstance(string name, Component definition)
    {
        InstanceName = name;
        Definition = definition;
    }

    public void OverrideParameter(string name, string value)
    {
        if (Definition.GetParameterByName(name) == null)
            throw new ArgumentException($"Parameter '{name}' not found in Component");

        OverriddenParameters[name] = value;
    }

    public string GetParameterValue(string name)
    {
        if (OverriddenParameters.TryGetValue(name, out var overrideVal))
            return overrideVal;

        var def = Definition.GetParameterByName(name);
        return def?.Value ?? throw new Exception($"Parameter '{name}' not found.");
    }

    public Dictionary<string, string> Resolve<T>() where T : IParameterAware
    {
        var members = Definition.GetMembersOfType<T>();
        var parameters = Definition.Parameters.ToDictionary(p => p.Name, p => GetParameterValue(p.Name));
        var resolver = new ExpressionResolver(parameters);
        return resolver.ResolveAll(members);
    }
}
// Add members of multiple types
uart.AddMember(new Register { Name = "CTRL", AddressExpression = "BASE + 0x0" });
uart.AddMember(new Interface { Name = "APB", OffsetExpression = "BASE + 0x8" });

// Resolve without needing custom methods:
var resolvedRegisters = inst2.Resolve<Register>();
var resolvedInterfaces = inst2.Resolve<Interface>();

Components with parameters and registers

System containing multiple component instances

Instance-level override of parameters

Parameter reference resolution in dependent members (e.g., register addresses/values)

