# scroll-recycler

This is a sample project demonstrating an optimization for Unity's ScrollRect object.

Unity's ScrollRect suffers from major performance hitches while scrolling after a very large number of children are added to the ScrollRect's content rect.

This optimization organizes the hierarchy of the ScrollRect and its children in a specific away to avoid these performance hitches while manually calculating some of Unity's UI operations to maintain the same end-user functionality.

## To come:
- Concrete detail on the cause of the performance hitch with ScrollRect in unity
- Detailed explanation of the optimization
