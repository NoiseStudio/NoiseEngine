namespace NoiseEngine.Physics.Collision;

internal struct ContactWithPointer<T> {

    public T Element;
    public int Pointer;

    public ContactWithPointer(T element, int pointer) {
        Element = element;
        Pointer = pointer;
    }

}
