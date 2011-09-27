%Global vars
lightCoord = [0 0 0];
lightColor = [1 0 1];
ambient = [.1 .1 .1];
cameraPos = [0 0 -10];
cameraLookAt = [0 0 0];
objectPos = [0 0 0];
objectOri = [0 0 0];
fov = 90.0;
near = 1;
far = 100;
ratio = 1;

A = importdata('shuttle_breneman_whitfield.raw', ' ', 0);
size = 1;
axis([-size size -size size]);
axis square;

%translate
projMatrix = [1 0 0 0;
    0 1 0 0;
    0 0 1 0;
    cameraLookAt - cameraPos 1];

ang = 0;
%X rotation
projMatrix = projMatrix * [1 0 0 0;
    0 cos(ang) sin(ang) 0;
    0 -sin(ang) cos(ang) 0;
    0 0 0 1];

%Y rotation
projMatrix = projMatrix * [cos(ang) 0 -sin(ang) 0
    0 1 0 0;
    sin(ang) 0 cos(ang) 0;
    0 0 0 1];

%Z rotation
ang = pi;
projMatrix = projMatrix * [ cos(ang) sin(ang) 0 0;
    -sin(ang) cos(ang) 0 0;
    0 0 1 0;
    0 0 0 1;];

%Projections
projMatrix = projMatrix * [1/ratio * cot(fov/2) 0 0 0;
    0 cot(fov/2) 0 0;
    0 0 far / (far - near) 1;
    0 0 -(far * near / (far - near)) 0];

for i = 1:length(A)
    %get the correct triangle
    points = [A(i, 1:3) 1; A(i, 4:6) 1; A(i, 7:9) 1];
    points = points * projMatrix;
    
    if points(1, 3) <= 0 || points(2, 3) <= 0 || points(3, 3) <= 0
        continue;
    end
    
    %Divide by W
    for j = 1:length(points(:,1))
        points(j, 1:3) = points(j, 1:3) / points(j, 4);
    end
    
    %back-cull
    v1 = points(2, 1:3) - points(1, 1:3);
    v2 = points(3, 1:3) - points(1, 1:3);
    normal = cross(v1, v2);
    dotProd = dot(normal, cameraPos - points(1, 1:3));
    if(dotProd < 0)
        patch(points(:,1), points(:,2), [1 0 0]);
    end
end