# mini-paint

An app which allows creating a picture with rectangles and ellipses.
Task description:

    Main window
        When the application starts the window is in the maximized state, filling an entire screen
        Font size is set to 16
    Layout
        The window is split into two parts
        Top part has 4 buttons, 2 on the left (Rectangle and Ellipse) and 2 on the right (Delete and Random colors)
        Top part background is LightGray
        The text on the buttons is wrapped to a new line if it does not fit in the button
        Lower part is filled with the Canvas control
        Canvas is filled with linear gradient from lower left corner of the application, to the upper right corner
        The gradient goes from the Black color, through the DarkSlateGray in the middle, to the Black color again
        The layout is preserved when the window is resized
    Displaying shapes
        When the application starts there are 4 random shapes displayed on the canvas (Rectangles and Ellipses)
        Positions, colors and sizes are randomized each time the application starts
        Shapes are not drawn outside of the canvas (they are clipped to a canvas area)
        Shapes are uniformly filled with color and have no outlines
    Selecting shapes
        When the mouse hover over a shape the cursor changes to a Hand
        Shapes can be selected and deselected by clicking on them with the right mouse button
        When a shape is selected it is displayed in front of all the unselected shapes
        Selected shape has a White glow effect with the radius of 50 pixels and direction of 270 degrees
    Changing shapes
        When the Delete button is pressed, all the selected shapes are removed from the canvas
        When the Random colors button is pressed, colors of all the selected shapes are changed to new randomly generated colors
    Adding shapes
        New shapes can be added by clicking either Rectangle button or Ellipse button
        When one of those buttons is pressed, the cursor over the canvas changes to a Cross
        New shapes can be drawn by pressing and holding the left mouse button on the canvas and moving the mouse
        During drawing the mouse can be moved anywhere, also outside of the application window, and the shape continues to follow the mouse cursor
        When the left mouse button is released, the shape is finished and the cursor changes back to the default one
    Top bar layout
        There are 2 additional buttons to the right of the application
        New buttons are separated by a black vertical line
        There are 4 additional controls with the shape properties
        There are 2 black vertical lines separating left and right sets of controls from the center
    Changing language
        On the right side of the top panel there is a button with an image of a flag
        Clicking on the button changes the language from English to Polish
        Flag image changes to match currently selected language
        All the strings and image references for given language are held in the corresponding resx files
        All the text in the application changes without application reloading
    Exporting image
        To the left of the button to change language there is a button that allows to export current canvas to an image file
        After pressing the button there is a dialog window displayed that allows to choose a name and location of a result image file
        The filter shows only .png files
        After pressing Save button the canvas with all the shapes is saved to the .png file
        The result file looks exactly like the canvas in the application
    Shape properties
        On the left side of the top panel, to the right of existing buttons there are 4 new controls stacked on top of each other. They are used to modify shape width, height, fill color and rotation angle
        Each field has a label on its left
        Labels are aligned to the right
        Width and height fields accept only positive integer values and the values are applied immediately to a shape
        Color can be selected from the list of all the colors found in System.Windows.Media.Colors, except for Transparent
        Angle label ends with currently selected angle integer value
        Angle can be changed with a slider with the values between -180 and 180 degrees
    Selecting shapes
        When the left mouse button is pressed on an unselected shape it is selected and all the other shapes are deselected
        When pressed anywhere on a canvas outside of any shape all the selected shapes are deselected
        Shape properties display correct properties of the last selected shape and update when changing selection
        Shape properties affect the last selected shape. When that shape is deselected, then properties affect the previous shape in the order of shape selection
        When no shape is selected, then the shape properties along with the Delete and Random colors buttons are disabled, and the properties are empty
    Moving shapes
        When pressing and holding the left mouse button on any selected shape all of them are moving with the mouse cursor
        Shapes keep the same position relative to a cursor
        When moving the shapes the cursor changes to a ScrollAll
        Shapes can be moved outside of the canvas
    Rotating shapes
        When moving the angle slider the selected shape is rotated by the selected angle
