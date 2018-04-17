using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

using EntityClasses;
using MapClasses;

public class EntityTests {

	[Test]
	public void Entity_Constructor_Clone() {
        // Use the Assert class to test conditions.
        Entity entity1 = new Entity { Index = 1, Name = "Test entity 1" };

        Entity entity2 = new Entity(entity1);

        Assert.AreEqual(entity1.Name, entity2.Name);
	}
    
    [Test]
    public void Entity_TurnLeft()
    {
        Direction directionStart = Direction.Up;
        Direction directionExpected = Direction.Left;
        Entity entity1 = new Entity { Facing = directionStart };

        entity1.TurnLeft();

        Assert.AreEqual(directionExpected, entity1.Facing);
    }

    [Test]
    public void Entity_TurnRight()
    {
        Direction directionStart = Direction.Up;
        Direction directionExpected = Direction.Right;
        Entity entity1 = new Entity { Facing = directionStart };

        entity1.TurnRight();

        Assert.AreEqual(directionExpected, entity1.Facing);
    }

    [Test]
    public void Entity_GetDirectionDegrees()
    {
        Direction direction = Direction.Right;
        float expectedDegrees = 90f;

        Entity entity1 = new Entity { Facing = direction };
        float degrees = entity1.GetDirectionDegrees();

        Assert.AreEqual(expectedDegrees, degrees);
    }

    [Test]
    public void Entity_GetAbsoluteDirection()
    {
        Direction facing = Direction.Right;
        Direction startDirection = Direction.Left;
        Direction expectedDirection = Direction.Up;

        Entity entity1 = new Entity { Facing = facing };
        Direction direction = entity1.GetDirection(startDirection);

        Assert.AreEqual(expectedDirection, direction);
    }

    [Test]
    public void Entity_GetBackward()
    {
        Direction directionStart = Direction.Right;
        Direction expectedDirection = Direction.Left;

        Entity entity1 = new Entity { Facing = directionStart };

        Direction direction = entity1.GetBackward();
        Assert.AreEqual(expectedDirection, direction);
    }

    [Test]
    public void Entity_GetLeft()
    {
        Direction directionStart = Direction.Right;
        Direction expectedDirection = Direction.Up;

        Entity entity1 = new Entity { Facing = directionStart };

        Direction direction = entity1.GetLeft();
        Assert.AreEqual(expectedDirection, direction);
    }

    [Test]
    public void Entity_GetRight()
    {
        Direction directionStart = Direction.Right;
        Direction expectedDirection = Direction.Down;

        Entity entity1 = new Entity { Facing = directionStart };

        Direction direction = entity1.GetRight();
        Assert.AreEqual(expectedDirection, direction);
    }

    [Test]
    public void Entity_Heal()
    {
        int startHealth = 50;
        int healAmount = 10;
        int expectedHealth = 60;

        Entity entity1 = new Entity { MaxHealth = 100, CurrentHealth = startHealth };
        entity1.Heal(healAmount);
        Assert.AreEqual(expectedHealth, entity1.CurrentHealth);
    }
    
    [Test]
    public void Entity_Damage()
    {
        int startHealth = 50;
        int damageAmount = 10;
        int expectedHealth = 40;

        Entity entity1 = new Entity { MaxHealth = 100, CurrentHealth = startHealth };
        entity1.Damage(damageAmount);
        Assert.AreEqual(expectedHealth, entity1.CurrentHealth);
    }
}
