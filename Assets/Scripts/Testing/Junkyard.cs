// Code snippets that aren't useful anymore but I was to scared to get rid off
//////////////////////////////////////////////////////////////////////
/*

    void addNodesToKDTree(Vector3[] points)
    {
        int oldCount = nodes.Count;
        nodes.SetCount(oldCount + points.Length);
        for (int i = oldCount, j = 0; i < nodes.Points.Length; i++, j++)
        {
            nodes.Points[i] = points[j];
        }
        nodes.Rebuild();

        if (debug == true)
        {
            Debug.Log("KDTree //nodes now contains new nodes at index " + oldCount + ". There are " + nodes.Count + " node(s) in total.");
        }
    }

    void InjectNodesToKDTree(Vector3 point, int splitIndex)
    {
        int oldCount = nodes.Count;
        Vector3[] shiftBuffer = new Vector3[oldCount + points.Length - splitIndex];
        nodes.SetCount(oldCount + points.Length);

        // Load new points into buffer
        for (int i = 0; i < points.Length; i++)
        {
            shiftBuffer[i] = points[i];
        }
        // Load shifted points from KD Tree after new points into buffer
        for (int i = points.Length, j = splitIndex; i < shiftBuffer.Length; i++, j++)
        {
            shiftBuffer[i] = nodes.Points[j];
        }
        // Write buffer into KD Tree
        for (int i = splitIndex, j = 0; i < nodes.Count; i++, j++)
        {
            nodes.Points[i] = shiftBuffer[j];
        }
        nodes.Rebuild();

        if (debug == true)
        {
            Debug.Log("KDTree //nodes now contains " + points.Length + " new node(s) at index " + splitIndex + ". There are " + nodes.Count + " node(s) in total.");
        }
    }

    void testRadiusFinder(int index)
    {
            var test = findInRadiusKDTree(index, searchRadius);
            foreach ( var x in test)
            {
                Debug.Log( x.ToString());
            }
    }
    
        void RepulsiveNodes(float scaleFactor, float maximumDistance, float queryRadius)
    {
        float distance;
        Vector3 currentToNext;

        Vector3[] distanceCheckPoints = null;
        for (int i = 0; i < nodes.Count; i++)
        {
            var resultIndices = new List<int>();
            query.Radius(nodes, nodes.Points[0], queryRadius, resultIndices); // In KDTree: Search for points within given radius
            if (resultIndices == null) return; // Skip if there are no points in radius proximity
            distanceCheckPoints = new Vector3[resultIndices.Count]; // Create array to write results into
            for (int j = 0; j < resultIndices.Count; j++)
            {
                distanceCheckPoints[j] = nodes.Points[resultIndices[j]]; // Write points into array
                currentToNext = distanceCheckPoints[(j + 1) % resultIndices.Count] - nodes.Points[i];
                distance = currentToNext.magnitude;
                if (distance < maximumDistance)
                {
                    nodes.Points[i] = nodes.Points[i] + (-currentToNext.normalized) * (maximumDistance - distance);
                    Debug.DrawRay(nodes.Points[i], currentToNext, Color.red, 1f);
                }
            }
        }
        nodes.Rebuild();
    }




    
    */