namespace NoiseEngine.Physics.Collision;

internal struct ContactPointWithPointer {

    public ContactPoint Point;
    public int Pointer;

    public ContactPointWithPointer(ContactPoint point, int pointer) {
        Point = point;
        Pointer = pointer;
    }

}
