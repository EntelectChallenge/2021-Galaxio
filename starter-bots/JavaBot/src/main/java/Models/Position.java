package Models;

public class Position {

  public int x;
  public int y;

  public Position() {
    x = 0;
    y = 0;
  }

  public Position(int x, int y) {
    this.x = x;
    this.y = y;
  }

  public int getX() {
    return x;
  }

  public void setX(int x) {
    x = x;
  }

  public int getY() {
    return y;
  }

  public void setY(int y) {
    this.y = y;
  }
}
