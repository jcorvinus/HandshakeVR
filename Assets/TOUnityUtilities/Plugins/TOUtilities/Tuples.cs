using System;


public struct Tuple { 
}
public struct Tuple<A> {
    public A Item1 { get; set; }

    public Tuple(A item1) {
        Item1 = item1;
    }
}
public struct Tuple<A, B> {
    public A Item1 { get; set; }
    public B Item2 { get; set; }

    public Tuple(A item1, B item2) {
        Item1 = item1;
        Item2 = item2;
    }
}
public struct Tuple<A, B, C> {
    public A Item1 { get; set; }
    public B Item2 { get; set; }
    public C Item3 { get; set; }

    public Tuple(A item1, B item2, C item3) {
        Item1 = item1;
        Item2 = item2;
        Item3 = item3;
    }
}
public struct Tuple<A, B, C, D> {
    public A Item1 { get; set; }
    public B Item2 { get; set; }
    public C Item3 { get; set; }
    public D Item4 { get; set; }

    public Tuple(A item1, B item2, C item3, D item4) {
        Item1 = item1;
        Item2 = item2;
        Item3 = item3;
        Item4 = item4;
    }
}
public struct Tuple<A, B, C, D, E> {
    public A Item1 { get; set; }
    public B Item2 { get; set; }
    public C Item3 { get; set; }
    public D Item4 { get; set; }
    public E Item5 { get; set; }

    public Tuple(A item1, B item2, C item3, D item4, E item5) {
        Item1 = item1;
        Item2 = item2;
        Item3 = item3;
        Item4 = item4;
        Item5 = item5;
    }
}
public struct Tuple<A, B, C, D, E, F> {
    public A Item1 { get; set; }
    public B Item2 { get; set; }
    public C Item3 { get; set; }
    public D Item4 { get; set; }
    public E Item5 { get; set; }
    public F Item6 { get; set; }

    public Tuple(A item1, B item2, C item3, D item4, E item5, F item6) {
        Item1 = item1;
        Item2 = item2;
        Item3 = item3;
        Item4 = item4;
        Item5 = item5;
        Item6 = item6;
    }
}

public static class TupleExt {
    

    public static Action<Tuple<A, B>> Pack<A, B>(this Action<A, B> act) {
        return x => act(x.Item1, x.Item2);
    }

    public static Action<Tuple<A, B, C>> Pack<A, B, C>(this Action<A, B, C> act) {
        return x => act(x.Item1, x.Item2, x.Item3);
    }

    public static Action<Tuple<A, B, C, D>> Pack<A, B, C, D>(this Action<A, B, C, D> act) {
        return x => act(x.Item1, x.Item2, x.Item3, x.Item4);
    }

    public static Action<A, B> Unpack<A, B>(this Action<Tuple<A, B>> act) {
        return (a, b) => act(new Tuple<A, B>(a, b));
    }

    public static Action<A, B, C> Unpack<A, B, C>(this Action<Tuple<A, B, C>> act) {
        return (a, b, c) => act(new Tuple<A, B, C>(a, b, c));
    }

    public static Action<A, B, C, D> Unpack<A, B, C, D>(this Action<Tuple<A, B, C, D>> act) {
        return (a, b, c, d) => act(new Tuple<A, B, C, D>(a, b, c, d));
    }
}

