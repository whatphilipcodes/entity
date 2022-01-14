// PARAMETERS
float _maxForce = 0.9;
float _maxSpeed = 1;
float _desiredSeparation = 30;
float _separationCohesionRation = 1.1;
float _maxEdgeLen = 8;
boolean seed = false;
//DifferentialLine _diff_line;
ArrayList<DifferentialLine> lines = new ArrayList<DifferentialLine>();


void setup() {
  frameRate(60);
  background(0);
  size(1280, 720);
}
void draw() {
  if (seed == true) {
    //println(frameCount);
    background(0);
    //_diff_line.run();
    //_diff_line.render();
    for (DifferentialLine line : lines) {
      line.run();
      line.render();
    }
  }
}
void mousePressed() {
  seed = true;
  DifferentialLine newLine = new DifferentialLine(_maxForce, _maxSpeed, _desiredSeparation, _separationCohesionRation, _maxEdgeLen);
  //drawseed();
    float nodesStart = 3;
  float angInc = TWO_PI/nodesStart;
  float rayStart = 5;
  for (float a=0; a<TWO_PI; a+=angInc) {
    float x = mouseX + cos(a) * rayStart;
    float y = mouseY + sin(a) * rayStart;
    newLine.addNode(new Node(x, y, newLine.maxForce, newLine.maxSpeed, newLine.desiredSeparation, newLine.separationCohesionRation));
    //println(mouseX);
    //println(mouseY);
  }
  lines.add(newLine);
}

/*void drawseed() {
  _diff_line = new DifferentialLine(_maxForce, _maxSpeed, _desiredSeparation, _separationCohesionRation, _maxEdgeLen);
  //fill(0);
  float nodesStart = 3;
  float angInc = TWO_PI/nodesStart;
  float rayStart = 5;
  for (float a=0; a<TWO_PI; a+=angInc) {
    float x = mouseX + cos(a) * rayStart;
    float y = mouseY + sin(a) * rayStart;
    _diff_line.addNode(new Node(x, y, _diff_line.maxForce, _diff_line.maxSpeed, _diff_line.desiredSeparation, _diff_line.separationCohesionRation));
    //println(mouseX);
    //println(mouseY);
  }
}*/



class DifferentialLine {
  ArrayList<Node> nodes;
  float maxForce;
  float maxSpeed;
  float desiredSeparation;
  float separationCohesionRation;
  float maxEdgeLen;
  DifferentialLine(float mF, float mS, float dS, float sCr, float eL) {
    nodes = new ArrayList<Node>();
    maxSpeed = mF;
    maxForce = mS;
    desiredSeparation = dS;
    separationCohesionRation = sCr;
    maxEdgeLen = eL;
  }
  void run() {
    for (Node n : nodes) {
      n.run(nodes);
    }
    growth();
  }
  void addNode(Node n) {
    nodes.add(n);
  }
  void addNodeAt(Node n, int index) {
    nodes.add(index, n);
  }
  void growth() {
    for (int i=0; i<nodes.size()-1; i++) {
      Node n1 = nodes.get(i);
      Node n2 = nodes.get(i+1);
      float d = PVector.dist(n1.position, n2.position);
      if (d>maxEdgeLen) { // Can add more rules for inserting nodes
        int index = nodes.indexOf(n2);
        PVector middleNode = PVector.add(n1.position, n2.position).div(2);
        addNodeAt(new Node(middleNode.x, middleNode.y, maxForce, maxSpeed, desiredSeparation, separationCohesionRation), index);
      }
    }
  }
  void render() {
    for (int i=0; i<nodes.size()-1; i++) {
      PVector p1 = nodes.get(i).position;
      PVector p2 = nodes.get(i+1).position;
      line(p1.x, p1.y, p2.x, p2.y);
      colorMode(HSB);
      stroke(255);
      //stroke(10,frameCount*0.3,100);
      //stroke(255);//Farbe
      strokeWeight(1); //Strichstärke
      if (i==nodes.size()-2) {
        line(p2.x, p2.y, nodes.get(0).position.x, nodes.get(0).position.y);
      }
    }
  }
}
class Node {
  PVector position;
  PVector velocity;
  PVector acceleration;
  float maxForce;
  float maxSpeed;
  float desiredSeparation;
  float separationCohesionRation;
  Node(float x, float y) {
    acceleration = new PVector(0, 0);
    velocity =PVector.random2D();
    position = new PVector(x, y);
  }
  Node(float x, float y, float mF, float mS, float dS, float sCr) {
    acceleration = new PVector(0, 0);
    velocity =PVector.random2D();
    position = new PVector(x, y);
    maxSpeed = mF;
    maxForce = mS;
    desiredSeparation = dS;
    separationCohesionRation = sCr;
  }
  void run(ArrayList<Node> nodes) {
    differentiate(nodes);
    update();
  }
  void applyForce(PVector force) {
    acceleration.add(force);
  }
  void differentiate(ArrayList<Node> nodes) {
    PVector separation = separate(nodes);
    PVector cohesion = edgeCohesion(nodes);
    separation.mult(separationCohesionRation);
    //cohesion.mult(1.0);
    applyForce(separation);
    applyForce(cohesion);
  }
  void update() {
    velocity.add(acceleration);
    velocity.limit(maxSpeed);
    position.add(velocity);
    acceleration.mult(0);
  }
  PVector seek(PVector target) {
    PVector desired = PVector.sub(target, position);
    desired.setMag(maxSpeed);
    PVector steer = PVector.sub(desired, velocity);
    steer.limit(maxForce);
    return steer;
  }
  PVector separate(ArrayList<Node> nodes) {
    PVector steer = new PVector(0, 0);
    int count = 0;
    for (Node other : nodes) {
      float d = PVector.dist(position, other.position);
      if (d>0 && d < desiredSeparation) {
        PVector diff = PVector.sub(position, other.position);
        diff.normalize();
        diff.div(d); // Weight by distance
        steer.add(diff);
        count++;
      }
    }
    if (count>0) {
      steer.div((float)count);
    }
    if (steer.mag() > 0) {
      steer.setMag(maxSpeed);
      steer.sub(velocity);
      steer.limit(maxForce);
    }
    return steer;
  }
  PVector edgeCohesion (ArrayList<Node> nodes) {
    PVector sum = new PVector(0, 0);
    int this_index = nodes.indexOf(this);
    if (this_index!=0 && this_index!=nodes.size()-1) {
      sum.add(nodes.get(this_index-1).position).add(nodes.get(this_index+1).position);
    } else if (this_index == 0) {
      sum.add(nodes.get(nodes.size()-1).position).add(nodes.get(this_index+1).position);
    } else if (this_index == nodes.size()-1) {
      sum.add(nodes.get(this_index-1).position).add(nodes.get(0).position);
    }
    sum.div(2);
    return seek(sum);
  }
}
