package Enums;

public enum ObjectTypes {
  PLAYER(1),
  FOOD(2),
  WORMHOLE(3),
  GAS_CLOUD(4),
  ASTEROID_FIELD(5);

  public final Integer value;

  ObjectTypes(Integer value) {
    this.value = value;
  }

  public static ObjectTypes valueOf(Integer value) {
    for (ObjectTypes objectType : ObjectTypes.values()) {
      if (objectType.value == value) return objectType;
    }

    throw new IllegalArgumentException("Value not found");
  }
}